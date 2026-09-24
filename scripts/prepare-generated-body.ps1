param(
    [Parameter(Mandatory = $true)] [string] $Source,
    [Parameter(Mandatory = $true)] [string] $Destination,
    [int] $CanvasSize = 256,
    [int] $BodyHeight = 194,
    [int] $BottomPadding = 26,
    [ValidateRange(1, 255)] [int] $AlphaThreshold = 10
)

Add-Type -AssemblyName System.Drawing

$sourceBitmap = [System.Drawing.Bitmap]::new($Source)
try {
    $left = $sourceBitmap.Width
    $top = $sourceBitmap.Height
    $right = -1
    $bottom = -1

    for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
        for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
            if ($sourceBitmap.GetPixel($x, $y).A -lt $AlphaThreshold) { continue }
            if ($x -lt $left) { $left = $x }
            if ($x -gt $right) { $right = $x }
            if ($y -lt $top) { $top = $y }
            if ($y -gt $bottom) { $bottom = $y }
        }
    }

    if ($right -lt $left -or $bottom -lt $top) {
        throw "Source image has no visible pixels."
    }

    $boundsWidth = $right - $left + 1
    $boundsHeight = $bottom - $top + 1
    $targetHeight = [Math]::Min($BodyHeight, $CanvasSize - $BottomPadding)
    $targetWidth = [Math]::Max(1, [int][Math]::Round($boundsWidth * $targetHeight / $boundsHeight))
    if ($targetWidth -gt $CanvasSize - 24) {
        $targetWidth = $CanvasSize - 24
        $targetHeight = [Math]::Max(1, [int][Math]::Round($boundsHeight * $targetWidth / $boundsWidth))
    }

    $targetX = [int][Math]::Round(($CanvasSize - $targetWidth) / 2)
    $targetY = $CanvasSize - $BottomPadding - $targetHeight
    $output = [System.Drawing.Bitmap]::new($CanvasSize, $CanvasSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($output)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.DrawImage(
                $sourceBitmap,
                [System.Drawing.Rectangle]::new($targetX, $targetY, $targetWidth, $targetHeight),
                [System.Drawing.Rectangle]::new($left, $top, $boundsWidth, $boundsHeight),
                [System.Drawing.GraphicsUnit]::Pixel)
        }
        finally { $graphics.Dispose() }

        $directory = [System.IO.Path]::GetDirectoryName($Destination)
        [System.IO.Directory]::CreateDirectory($directory) | Out-Null
        $output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $output.Dispose() }

    Write-Output "sourceBounds=$left,$top,$boundsWidth,$boundsHeight runtimeRect=$targetX,$targetY,$targetWidth,$targetHeight"
}
finally { $sourceBitmap.Dispose() }
