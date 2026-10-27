<#
    Regenerates the browser tab / app icons from the store's teal logo so the
    favicon matches the site artwork.

    Source : lern/wwwroot/assets/images/mountainous-logo-transparent.png
    Output : lern/wwwroot/assets/images/favicon_io/*  (+ /favicon.ico)
    Originals are backed up to favicon_io/_original first.
#>
param(
    [string]$SourceLogo = "lern/wwwroot/assets/images/mountainous-logo-transparent.png",
    [string]$OutputDir = "lern/wwwroot/assets/images/favicon_io"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$logoPath = Join-Path $root $SourceLogo
$outDir = Join-Path $root $OutputDir
if (-not (Test-Path $logoPath)) { throw "Logo not found: $logoPath" }

$backup = Join-Path $outDir "_original"
New-Item -ItemType Directory -Force $backup, $outDir | Out-Null

$logo = [System.Drawing.Image]::FromFile($logoPath)

function New-IconBitmap {
    param([int]$Size, [double]$Fill, [switch]$Transparent)
    $bmp = New-Object System.Drawing.Bitmap($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    if ($Transparent) { $g.Clear([System.Drawing.Color]::Transparent) }
    else { $g.Clear([System.Drawing.Color]::White) }
    $scale = [Math]::Min($Size * $Fill / $logo.Width, $Size * $Fill / $logo.Height)
    $w = [int][Math]::Round($logo.Width * $scale)
    $h = [int][Math]::Round($logo.Height * $scale)
    $x = [int](($Size - $w) / 2)
    $y = [int](($Size - $h) / 2)
    $g.DrawImage($logo, $x, $y, $w, $h)
    $g.Dispose()
    return $bmp
}

function Save-Png {
    param([System.Drawing.Bitmap]$Bitmap, [string]$Name)
    $path = Join-Path $outDir $Name
    if (Test-Path $path) {
        $existing = Join-Path $backup $Name
        if (-not (Test-Path $existing)) { Move-Item -LiteralPath $path -Destination $existing }
    }
    $Bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host ("  {0,-28} {1,6:N0} bytes" -f $Name, (Get-Item $path).Length)
}

Write-Host "Generating favicons from $SourceLogo"
# Small tab icons stay transparent so they adapt to light/dark browser chrome;
# the larger OS/app icons get a solid white plate (iOS shows black otherwise).
$specs = @(
    @{ Size = 16;  Name = "favicon-16x16.png";         Fill = 0.94; Transparent = $true  },
    @{ Size = 32;  Name = "favicon-32x32.png";         Fill = 0.92; Transparent = $true  },
    @{ Size = 48;  Name = "favicon-48x48.png";         Fill = 0.90; Transparent = $true  },
    @{ Size = 180; Name = "apple-touch-icon.png";      Fill = 0.80; Transparent = $false },
    @{ Size = 192; Name = "android-chrome-192x192.png"; Fill = 0.80; Transparent = $false },
    @{ Size = 512; Name = "android-chrome-512x512.png"; Fill = 0.80; Transparent = $false }
)
foreach ($spec in $specs) {
    if ($spec.Transparent) { $bmp = New-IconBitmap -Size $spec.Size -Fill $spec.Fill -Transparent }
    else { $bmp = New-IconBitmap -Size $spec.Size -Fill $spec.Fill }
    Save-Png -Bitmap $bmp -Name $spec.Name
    $bmp.Dispose()
}

# --- Write favicon.ico (PNG-encoded entries: 16, 32, 48) ---------------------
$icoSizes = 16, 32, 48
$pngBytes = @()
foreach ($size in $icoSizes) {
    $bmp = New-IconBitmap -Size $size -Fill 0.92 -Transparent
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes += ,$ms.ToArray()
    $ms.Dispose(); $bmp.Dispose()
}

$icoPath = Join-Path $outDir "favicon.ico"
if (Test-Path $icoPath) {
    $existing = Join-Path $backup "favicon.ico"
    if (-not (Test-Path $existing)) { Move-Item -LiteralPath $icoPath -Destination $existing }
}

$fs = [System.IO.File]::Create($icoPath)
$bw = New-Object System.IO.BinaryWriter($fs)
try {
    $bw.Write([UInt16]0)                 # reserved
    $bw.Write([UInt16]1)                 # type: icon
    $bw.Write([UInt16]$icoSizes.Count)   # image count
    $offset = 6 + (16 * $icoSizes.Count)
    for ($i = 0; $i -lt $icoSizes.Count; $i++) {
        $size = $icoSizes[$i]
        $bw.Write([byte]$(if ($size -ge 256) { 0 } else { $size }))  # width
        $bw.Write([byte]$(if ($size -ge 256) { 0 } else { $size }))  # height
        $bw.Write([byte]0)               # palette colors
        $bw.Write([byte]0)               # reserved
        $bw.Write([UInt16]1)             # color planes
        $bw.Write([UInt16]32)            # bits per pixel
        $bw.Write([UInt32]$pngBytes[$i].Length)
        $bw.Write([UInt32]$offset)
        $offset += $pngBytes[$i].Length
    }
    foreach ($data in $pngBytes) { $bw.Write($data) }
} finally {
    $bw.Dispose(); $fs.Dispose()
}
Write-Host ("  {0,-28} {1,6:N0} bytes" -f "favicon.ico", (Get-Item $icoPath).Length)

# Browsers request /favicon.ico by default; keep a copy at the site root.
$rootIco = Join-Path $root "lern/wwwroot/favicon.ico"
Copy-Item -LiteralPath $icoPath -Destination $rootIco -Force
Write-Host ("  {0,-28} {1,6:N0} bytes" -f "wwwroot/favicon.ico", (Get-Item $rootIco).Length)

$logo.Dispose()
Write-Host "Done. Originals kept in: $backup"
