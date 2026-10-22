[CmdletBinding()]
param(
    [string]$TargetFolder = ".",
    [string]$RootPath = "",
    [switch]$ExportJson = $false
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Resolve target directory dynamically
$targetDir = $PWD.Path
if ($TargetFolder -and $TargetFolder -ne ".") {
    $targetDir = Join-Path $PWD.Path $TargetFolder
} elseif ($RootPath) {
    if (Test-Path (Join-Path $RootPath "src\Frontend")) {
        $targetDir = Join-Path $RootPath "src\Frontend"
    } else {
        $targetDir = $RootPath
    }
}
$targetDir = [System.IO.Path]::GetFullPath($targetDir)

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  📱 Socratic Touch & POS/Kiosk Ergonomics Auditor" -ForegroundColor Cyan
Write-Host "  Target: $targetDir" -ForegroundColor DarkGray
Write-Host "==================================================" -ForegroundColor Cyan

if (-not (Test-Path $targetDir)) {
    Write-Error "Target directory not found at: $targetDir"
    return
}

# Regex patterns for ergonomics violations
# 1. Height under 44px on buttons or interactive inputs
$smallHeightRegex = [regex]'(?:height|min-height)\s*:\s*([1-3][0-9]|4[0-3])px\b'

# 2. Width under 44px on button/icon-button
$smallWidthRegex = [regex]'(?:width|min-width)\s*:\s*([1-3][0-9]|4[0-3])px\b'

# 3. Tiny fonts (< 10px / 0.65rem)
$tinyFontRegex = [regex]'font-size\s*:\s*(?:[1-9]px|0\.[1-6][0-9]*rem)\b'

# Excluded patterns (e.g. dividers, borders, icons inside buttons, badges, skeletons)
$excludePatterns = @(
    'border',
    'divider',
    'line-height',
    'stroke',
    'pos-badge-qty',
    'badge',
    'avatar',
    'scrollbar',
    'skeleton'
)

$files = Get-ChildItem -Path $targetDir -Recurse -File -Include "*.razor", "*.razor.css", "*.css" |
    Where-Object {
        $_.FullName -notmatch '\\(bin|obj|\.git|\.vscode|\.agents|node_modules|wwwroot\\lib)\\'
    }

$violations = @()

foreach ($file in $files) {
    $lines = @(Get-Content $file.FullName -Encoding UTF8 -ErrorAction SilentlyContinue)
    if ($lines.Count -eq 0) { continue }

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $lineNum = $i + 1
        $line = [string]$lines[$i]

        # Skip comment lines
        if ($line.Trim().StartsWith("//") -or $line.Trim().StartsWith("/*") -or $line.Trim().StartsWith("*")) {
            continue
        }

        # Check if line contains exclusions
        $skip = $false
        foreach ($exc in $excludePatterns) {
            if ($line -match $exc) {
                $skip = $true
                break
            }
        }
        if ($skip) { continue }

        # Check small height
        if ($line -match $smallHeightRegex) {
            $val = $matches[1]
            $violations += [PSCustomObject]@{
                File = $file.FullName
                RelativePath = [System.IO.Path]::GetRelativePath($targetDir, $file.FullName)
                Line = $lineNum
                IssueType = "SmallTouchTargetHeight"
                Snippet = "$($val)px"
                LineContent = $line.Trim()
                Recommendation = "Increase height to >= 48px (--md-sys-touch-target-min) for POS or >= 56px for Kiosks."
            }
            continue
        }

        # Check tiny font
        if ($line -match $tinyFontRegex) {
            $violations += [PSCustomObject]@{
                File = $file.FullName
                RelativePath = [System.IO.Path]::GetRelativePath($targetDir, $file.FullName)
                Line = $lineNum
                IssueType = "TinyUnreadableFont"
                Snippet = $matches[0]
                LineContent = $line.Trim()
                Recommendation = "Use readable font size >= 12px / 0.75rem for POS & Kiosks to guarantee outdoor/counter legibility."
            }
            continue
        }
    }
}

Write-Host "Found $($violations.Count) potential touch ergonomics issue(s) across $($files.Count) files." -ForegroundColor $(if ($violations.Count -eq 0) { "Green" } else { "Yellow" })

if ($violations.Count -gt 0) {
    $violations | Select-Object -First 20 | ForEach-Object {
        Write-Host "  [$($_.IssueType)] $($_.RelativePath):$($_.Line)" -ForegroundColor Yellow
        Write-Host "    Found: $($_.LineContent)" -ForegroundColor DarkGray
        Write-Host "    Fix:   $($_.Recommendation)" -ForegroundColor Cyan
    }
    if ($violations.Count -gt 20) {
        Write-Host "  ... and $($violations.Count - 20) more issue(s)." -ForegroundColor DarkGray
    }
} else {
    Write-Host "  ✅ All checked components and styles comply with Socratic Touch Ergonomics!" -ForegroundColor Green
}

if ($ExportJson) {
    $violations | ConvertTo-Json -Depth 3
}
