<#
    Shrinks the oversized home-slider banner images.

    The banner art was authored as 2-3 MB lossless PNG. These are photographic
    banners, so re-encoding to progressive JPEG at a sensible width keeps them
    looking good while cutting the payload ~20-30x.

    Steps:
      1. Convert each large slider PNG to .jpg (original kept in _original/).
      2. Emit a SQL file that repoints SiteBanners.ImageUrl / MobileImageUrl
         from .png to .jpg, to run on the local and host databases.

    Usage:
        powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\optimize-slider-images.ps1
#>
param(
    [string]$SliderDir = "lern/wwwroot/assets/images/slider",
    [int]$MaxDesktopWidth = 1600,
    [int]$MaxMobileWidth = 900,
    [int]$DesktopQuality = 80,
    [int]$MobileQuality = 78,
    [int]$MinBytes = 400000,
    [string]$SqlOut = "scripts/update-banner-images.sql"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$dir = Join-Path $root $SliderDir
if (-not (Test-Path $dir)) { throw "Slider directory not found: $dir" }

$backup = Join-Path $dir "_original"
New-Item -ItemType Directory -Force $backup | Out-Null

$jpgCodec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() |
    Where-Object { $_.MimeType -eq 'image/jpeg' } | Select-Object -First 1

function Save-Jpeg {
    param([System.Drawing.Image]$Image, [int]$Width, [string]$Path, [int]$Quality)
    $height = [int][Math]::Round($Image.Height * $Width / $Image.Width)
    $bmp = New-Object System.Drawing.Bitmap($Width, $height, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.Clear([System.Drawing.Color]::White)
    $g.DrawImage($Image, 0, 0, $Width, $height)
    $g.Dispose()
    $encParams = New-Object System.Drawing.Imaging.EncoderParameters(1)
    $encParams.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, [long]$Quality)
    $bmp.Save($Path, $jpgCodec, $encParams)
    $bmp.Dispose()
}

$targets = Get-ChildItem $dir -File | Where-Object {
    $_.Length -gt $MinBytes -and $_.Extension -match '^\.(png|jpg|jpeg)$'
}

if (-not $targets) { Write-Host "Nothing to optimize (no files over $([math]::Round($MinBytes/1KB)) KB)."; return }

$totalBefore = 0
$totalAfter = 0
$renamed = @()

foreach ($file in $targets) {
    $isMobile = $file.Name -match 'mobile'
    $maxWidth = if ($isMobile) { $MaxMobileWidth } else { $MaxDesktopWidth }
    $quality = if ($isMobile) { $MobileQuality } else { $DesktopQuality }
    $target = Join-Path $file.DirectoryName ($file.BaseName + ".jpg")

    $img = [System.Drawing.Image]::FromFile($file.FullName)
    try { Save-Jpeg -Image $img -Width ([Math]::Min($img.Width, $maxWidth)) -Path $target -Quality $quality }
    finally { $img.Dispose() }

    $backupPath = Join-Path $backup $file.Name
    if (-not (Test-Path $backupPath)) { Move-Item -LiteralPath $file.FullName -Destination $backupPath }
    elseif (Test-Path $file.FullName) { Remove-Item -LiteralPath $file.FullName -Force }

    $before = (Get-Item $backupPath).Length
    $after = (Get-Item $target).Length
    $totalBefore += $before; $totalAfter += $after
    if ($file.Extension -ne ".jpg") { $renamed += $file.Name }
    Write-Host ("{0,-34} {1,7:N0} KB -> {2,6:N0} KB  ({3}x smaller)" -f `
        (Split-Path $target -Leaf), ($before/1KB), ($after/1KB), [Math]::Round($before / $after, 1))
}

Write-Host ""
Write-Host ("Total: {0:N2} MB -> {1:N2} MB ({2}x smaller)" -f ($totalBefore/1MB), ($totalAfter/1MB), [Math]::Round($totalBefore / $totalAfter, 1))
Write-Host "Originals kept in: $backup"

# --- SQL to repoint banner rows from .png to .jpg ---------------------------
# Databases run this with a REPLACE over the three slider paths.
$sqlPath = Join-Path $root $SqlOut
@"
-- Repoint home-slider banners from the old PNG art to the optimized JPEGs.
-- Safe to run more than once.
UPDATE dbo.SiteBanners
SET ImageUrl = REPLACE(ImageUrl, '.png', '.jpg')
WHERE ImageUrl LIKE '/assets/images/slider/%.png';

UPDATE dbo.SiteBanners
SET MobileImageUrl = REPLACE(MobileImageUrl, '.png', '.jpg')
WHERE MobileImageUrl LIKE '/assets/images/slider/%.png';
"@ | Set-Content -Path $sqlPath -Encoding UTF8
Write-Host "SQL written: $sqlPath"
