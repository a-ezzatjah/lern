[CmdletBinding()]
param(
  [string]$Configuration = "Release",
  [string]$Project = "lern/lern.csproj",
  [string]$PublishDir = "artifacts/publish",
  [string]$PackageDir = "artifacts/packages",
  [string]$FtpHost = "irwin1.euhosted.com",
  [int]$FtpPort = 21,
  # The FTP account's home directory already IS the site root, so the upload
  # target is the FTP root ("/"). Do not use "/httpdocs" here or you will end
  # up uploading into "/httpdocs/httpdocs".
  [string]$RemotePath = "/",
  # FTPS (FTP over explicit TLS on port 21) is the default so credentials and
  # files are not sent in clear text. Use -NoFtps only if the host rejects it.
  [switch]$NoFtps,
  # Shared hosts often use a self-signed TLS certificate. Passing this switch
  # accepts it. Only use it when you trust the network you are deploying from.
  [switch]$AllowInvalidCertificate,
  [switch]$UseAppOffline,
  # wwwroot/uploads holds user/editor uploaded media (products, banners,
  # profiles, articles). Uploading it every deploy overwrites whatever exists
  # on the server. Pass -SkipUploads to leave the server's media untouched.
  [switch]$SkipUploads,
  # Build and package locally, connect and verify credentials, but do not
  # upload or modify anything on the server.
  [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$useFtps = -not $NoFtps

if ($useFtps) {
  [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
  if ($AllowInvalidCertificate) {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    Write-Warning "TLS certificate validation is disabled for this run (-AllowInvalidCertificate)."
  }
}

$publishPath = Join-Path $root $PublishDir
$packagePath = Join-Path $root $PackageDir
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$zipPath = Join-Path $packagePath "lern-$stamp.zip"

Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $publishPath,$packagePath | Out-Null

dotnet publish $Project -c $Configuration -o $publishPath --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

# Keep runtime data/configuration out of the deploy package.
# The host keeps its own appsettings.json and uploads/ on the server.
Remove-Item (Join-Path $publishPath "appsettings*.json") -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $publishPath "uploads") -Recurse -Force -ErrorAction SilentlyContinue

# The server's web.config carries the production connection string and
# environment variables (ASPNETCORE_ENVIRONMENT, Database__*, etc.). The copy
# produced by publish has none of these, so uploading it would take the site
# down. Preserve the server's file.
Remove-Item (Join-Path $publishPath "web.config") -Force -ErrorAction SilentlyContinue

if ($SkipUploads) {
  # User-uploaded media lives here; drop it so the server's copy is preserved.
  Remove-Item (Join-Path $publishPath "wwwroot\uploads") -Recurse -Force -ErrorAction SilentlyContinue
  Write-Host "Skipping wwwroot/uploads (server media preserved)."
}

Compress-Archive -Path (Join-Path $publishPath "*") -DestinationPath $zipPath -Force
Write-Host "Package created: $zipPath"
Write-Host "SHA256: $((Get-FileHash $zipPath -Algorithm SHA256).Hash)"

# --- FTP helpers -------------------------------------------------------------

function Get-FtpBase {
  $path = $RemotePath.TrimEnd('/')
  return "ftp://$FtpHost`:$FtpPort$path"
}

function New-FtpRequest {
  param([string]$Uri, [string]$Method, [pscredential]$Credential)
  $request = [System.Net.FtpWebRequest]::Create($Uri)
  $request.Method = $Method
  $request.Credentials = $Credential
  $request.UseBinary = $true
  $request.KeepAlive = $false
  $request.EnableSsl = $useFtps
  $request.Timeout = 60000
  return $request
}

function Test-FtpDirectory {
  param([string]$Base, [string]$RelativeDir, [pscredential]$Credential)
  $uri = if ([string]::IsNullOrEmpty($RelativeDir)) { "$Base/" } else { "$Base/$RelativeDir/" }
  try {
    $request = New-FtpRequest -Uri $uri -Method ([System.Net.WebRequestMethods+Ftp]::ListDirectory) -Credential $Credential
    $response = $request.GetResponse()
    $response.Close()
    return $true
  } catch {
    return $false
  }
}

function New-FtpDirectory {
  param([string]$Base, [string]$RelativeDir, [pscredential]$Credential)
  if ([string]::IsNullOrEmpty($RelativeDir)) { return }
  # Create parents first so a nested new folder works.
  $parts = $RelativeDir.Replace('\','/').Split('/', [System.StringSplitOptions]::RemoveEmptyEntries)
  $accumulated = ""
  foreach ($part in $parts) {
    $accumulated = if ([string]::IsNullOrEmpty($accumulated)) { $part } else { "$accumulated/$part" }
    if (-not (Test-FtpDirectory -Base $Base -RelativeDir $accumulated -Credential $Credential)) {
      $request = New-FtpRequest -Uri "$Base/$accumulated" -Method ([System.Net.WebRequestMethods+Ftp]::MakeDirectory) -Credential $Credential
      $response = $request.GetResponse()
      $response.Close()
      Write-Host "  created dir: $accumulated"
    }
  }
}

function Send-FtpFile {
  param([string]$Base, [string]$RelativePath, [string]$LocalPath, [pscredential]$Credential)
  # Retry transient failures (hosts often reject a transfer while a previous
  # connection is still closing). Tls12 + fresh request each attempt.
  $attempts = 3
  for ($attempt = 1; $attempt -le $attempts; $attempt++) {
    try {
      $request = New-FtpRequest -Uri "$Base/$RelativePath" -Method ([System.Net.WebRequestMethods+Ftp]::UploadFile) -Credential $Credential
      $stream = [System.IO.File]::OpenRead($LocalPath)
      try {
        $upload = $request.GetRequestStream()
        try { $stream.CopyTo($upload) } finally { $upload.Dispose() }
      } finally { $stream.Dispose() }
      return $true
    } catch {
      if ($attempt -eq $attempts) { throw }
      Start-Sleep -Seconds (2 * $attempt)
    }
  }
}

function Remove-FtpFile {
  param([string]$Base, [string]$RelativePath, [pscredential]$Credential)
  try {
    $request = New-FtpRequest -Uri "$Base/$RelativePath" -Method ([System.Net.WebRequestMethods+Ftp]::DeleteFile) -Credential $Credential
    $response = $request.GetResponse()
    $response.Close()
    return $true
  } catch {
    return $false
  }
}

# --- Credentials -------------------------------------------------------------
# Prefer the environment so the password is never typed/pasted into a shell or
# chat: set $env:LERN_FTP_USER and $env:LERN_FTP_PASSWORD before running.

$ftpUser = $env:LERN_FTP_USER
$ftpPassword = $env:LERN_FTP_PASSWORD

if ([string]::IsNullOrWhiteSpace($ftpUser) -or [string]::IsNullOrWhiteSpace($ftpPassword)) {
  $credential = Get-Credential -Message "FTP credentials for $FtpHost (user $ftpUser)"
} else {
  $secure = ConvertTo-SecureString $ftpPassword -AsPlainText -Force
  $credential = New-Object System.Management.Automation.PSCredential($ftpUser, $secure)
}

$base = Get-FtpBase
Write-Host "Uploading to $base (FTPS: $useFtps)"

$files = Get-ChildItem $publishPath -Recurse -File

if ($DryRun) {
  Write-Host ""
  Write-Host "DRY RUN: no files will be uploaded."
  Write-Host "  FTP target     : $base"
  Write-Host "  FTPS           : $useFtps"
  Write-Host "  Files to upload: $($files.Count)"
  try {
    $probe = New-FtpRequest -Uri "$base/" -Method ([System.Net.WebRequestMethods+Ftp]::ListDirectory) -Credential $credential
    $resp = $probe.GetResponse()
    $resp.Close()
    Write-Host "  FTP login      : OK (credentials valid, root reachable)"
  } catch {
    Write-Host "  FTP login      : FAILED -> $($_.Exception.Message)"
    throw "Dry run could not authenticate to the FTP server."
  }
  Write-Host "Dry run complete."
  return
}

# Create every directory that the payload needs before uploading files.
$dirs = $files | ForEach-Object {
  $rel = $_.FullName.Substring($publishPath.Length).TrimStart('\').Replace('\','/')
  Split-Path $rel -Parent
} | Where-Object { $_ -and $_ -ne "." } | Sort-Object -Unique

foreach ($dir in $dirs) {
  New-FtpDirectory -Base $base -RelativeDir $dir -Credential $credential
}

if ($UseAppOffline) {
  $offlineLocal = Join-Path $env:TEMP "app_offline.htm"
  @"
<!DOCTYPE html>
<html><head><meta charset="utf-8"><title>Updating</title></head>
<body style="font-family:sans-serif;text-align:center;padding:4rem">
<h1>در حال به‌روزرسانی سایت</h1>
<p>لطفاً چند لحظه بعد دوباره تلاش کنید.</p>
</body></html>
"@ | Set-Content -Path $offlineLocal -Encoding UTF8
  Send-FtpFile -Base $base -RelativePath "app_offline.htm" -LocalPath $offlineLocal -Credential $credential
  Write-Host "Placed app_offline.htm (site is temporarily offline)."
}

$failed = @()
$i = 0
foreach ($file in $files) {
  $relative = $file.FullName.Substring($publishPath.Length).TrimStart('\').Replace('\','/')
  try {
    Send-FtpFile -Base $base -RelativePath $relative -LocalPath $file.FullName -Credential $credential | Out-Null
    $i++
  } catch {
    $failed += $relative
    Write-Warning "FAILED  $relative  -> $($_.Exception.Message)"
  }
}
Write-Host "Uploaded $i of $($files.Count) files to $base"

if ($failed.Count -gt 0) {
  Write-Warning "Could not upload $($failed.Count) file(s):"
  $failed | ForEach-Object { Write-Warning "  $_" }
  Write-Warning "If any are .dll/.exe files that the site still has loaded, upload them from Plesk File Manager."
} else {
  Write-Host "All files uploaded successfully."
}

# Bring the site back online no matter what happened above.
if ($UseAppOffline) {
  if (Remove-FtpFile -Base $base -RelativePath "app_offline.htm" -Credential $credential) {
    Write-Host "Removed app_offline.htm (site is back online)."
  } else {
    Write-Warning "Could not remove app_offline.htm automatically."
    Write-Warning "Delete /app_offline.htm in Plesk File Manager to bring the site back online."
  }
}
