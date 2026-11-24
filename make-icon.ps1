# A4Tech Companion exe/tray ikonunu uretir (16/32/48/64 px, 32-bit alpha)
Add-Type -AssemblyName System.Drawing

$outDir = Join-Path $PSScriptRoot 'Resources'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$outFile = Join-Path $outDir 'app.ico'

function New-A4Bitmap([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    $m = [math]::Max(1, [int]($size * 0.045))
    $rect = New-Object System.Drawing.Rectangle($m, $m, ($size - 2 * $m), ($size - 2 * $m))
    $r = [math]::Max(2, [int]($size * 0.22))

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($rect.X, $rect.Y, $r, $r, 180, 90)
    $path.AddArc($rect.Right - $r, $rect.Y, $r, $r, 270, 90)
    $path.AddArc($rect.Right - $r, $rect.Bottom - $r, $r, $r, 0, 90)
    $path.AddArc($rect.X, $rect.Bottom - $r, $r, $r, 90, 90)
    $path.CloseFigure()

    $bg = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 36, 40, 48))
    $border = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 255, 165, 55), ([math]::Max(1, [int]($size * 0.045))))
    $g.FillPath($bg, $path)
    $g.DrawPath($border, $path)

    $font = New-Object System.Drawing.Font('Segoe UI', ([single]($size * 0.42)), [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 175, 60))
    $fmt = New-Object System.Drawing.StringFormat
    $fmt.Alignment = [System.Drawing.StringAlignment]::Center
    $fmt.LineAlignment = [System.Drawing.StringAlignment]::Center
    $g.DrawString('A4', $font, $brush, (New-Object System.Drawing.RectangleF(0, ([single]($size * 0.04)), $size, $size)), $fmt)

    $g.Dispose(); $font.Dispose(); $brush.Dispose(); $bg.Dispose(); $border.Dispose(); $path.Dispose(); $fmt.Dispose()
    return $bmp
}

function ConvertTo-IcoImage([System.Drawing.Bitmap]$bmp) {
    $w = $bmp.Width; $h = $bmp.Height
    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter($ms)

    # BITMAPINFOHEADER
    $bw.Write([uint32]40)
    $bw.Write([int32]$w)
    $bw.Write([int32]($h * 2))
    $bw.Write([uint16]1)
    $bw.Write([uint16]32)
    $bw.Write([uint32]0)
    $bw.Write([uint32]0)
    $bw.Write([int32]0); $bw.Write([int32]0); $bw.Write([uint32]0); $bw.Write([uint32]0)

    # BGRA, alt satirdan yukari
    for ($y = $h - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $w; $x++) {
            $c = $bmp.GetPixel($x, $y)
            $bw.Write([byte]$c.B); $bw.Write([byte]$c.G); $bw.Write([byte]$c.R); $bw.Write([byte]$c.A)
        }
    }

    # AND mask (32-bit alpha ikonunda sifir)
    $rowBytes = [int]([math]::Ceiling($w / 32.0) * 4)
    for ($y = 0; $y -lt $h; $y++) {
        for ($b = 0; $b -lt $rowBytes; $b++) { $bw.Write([byte]0) }
    }
    return $ms.ToArray()
}

$sizes = @(16, 32, 48, 64)
$images = @()
foreach ($s in $sizes) {
    $bmp = New-A4Bitmap $s
    $imgBytes = [byte[]](ConvertTo-IcoImage $bmp)
    Write-Output "  $s px -> $($imgBytes.Length) byte"
    $images += ,$imgBytes
    $bmp.Dispose()
}

$stream = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($stream)
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]$sizes.Count)

$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]
    $data = $images[$i]
    $writer.Write([byte]$s)
    $writer.Write([byte]$s)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]32)
    $writer.Write([uint32]$data.Length)
    $writer.Write([uint32]$offset)
    $offset += $data.Length
}
foreach ($data in $images) { $writer.Write($data) }

[System.IO.File]::WriteAllBytes($outFile, $stream.ToArray())
$writer.Dispose(); $stream.Dispose()
Write-Output "Olusturuldu: $outFile"
