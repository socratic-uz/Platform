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
$repoRoot = $targetDir

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  🎨 Socratic Theme & Accent Colors Auditor" -ForegroundColor Cyan
Write-Host "  Target: $targetDir" -ForegroundColor DarkGray
Write-Host "==================================================" -ForegroundColor Cyan

if (-not (Test-Path $targetDir)) {
    Write-Error "Target directory not found at: $targetDir"
    return
}

# Regex patterns
# 1. Hex colors (#fff, #ffffff, #12121280) excluding url(#id)
$hexRegex = [regex]'(?<!url\()#([0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\b'

# 2. RGB/RGBA values
$rgbRegex = [regex]'rgba?\(\s*\d+\s*,\s*\d+\s*,\s*\d+(?:\s*,\s*[\d.]+%?)?\s*\)'

# 3. Named colors in CSS property declarations
$namedCssRegex = [regex]'(?:color|background|background-color|border|border-color|fill|stroke)\s*:\s*\b(white|black|red|green|blue|gray|grey)\b'

# 4. Tailwind color classes
$tailwindColorRegex = [regex]'\b(text-white|text-black|bg-white|bg-black|bg-gray-\d+|text-gray-\d+|border-gray-\d+|bg-blue-\d+|text-blue-\d+)\b'

# Mapping suggestions based on context
function Get-Suggestion([string]$snippet, [string]$lineContent) {
    $lowerSnippet = $snippet.ToLower()
    $lowerLine = $lineContent.ToLower()

    if ($lowerLine -match 'border') {
        return "var(--md-sys-color-outline) or var(--md-sys-color-outline-variant)"
    }
    if ($lowerSnippet -in @('#fff', '#ffffff', 'white') -or $lowerSnippet -match '255,\s*255,\s*255') {
        if ($lowerLine -match 'background') {
            return "var(--md-sys-color-surface) or var(--md-sys-color-surface-container-lowest)"
        }
        return "var(--md-sys-color-on-surface) or var(--md-sys-color-on-primary)"
    }
    if ($lowerSnippet -in @('#000', '#000000', '#121212', '#1e1e1e', 'black') -or $lowerSnippet -match '0,\s*0,\s*0') {
        if ($lowerLine -match 'background') {
            return "var(--md-sys-color-surface) or var(--md-sys-color-surface-container)"
        }
        return "var(--md-sys-color-on-surface) or var(--md-sys-color-shadow)"
    }
    if ($lowerSnippet -in @('#00ffb3', '#3b82f6', '#10b981', '#06b6d4', '#6366f1') -or $lowerLine -match 'accent|primary') {
        return "var(--md-sys-color-primary) or var(--md-sys-color-primary-container)"
    }
    if ($lowerSnippet -in @('#ef4444', '#dc2626', 'red') -or $lowerLine -match 'error|danger') {
        return "var(--md-sys-color-error) or var(--md-sys-color-error-container)"
    }
    return "var(--md-sys-color-*)"
}

$files = Get-ChildItem -Path $targetDir -Include "*.razor", "*.css" -Recurse -File

$findings = @()

foreach ($file in $files) {
    # Skip obj, bin, and the theme definitions themselves
    if ($file.FullName -match '[\\/](obj|bin)[\\/]' -or 
        $file.FullName -match '[\\/]material-theme\.css[\\/]' -or
        $file.FullName -match 'Color\.razor\.js' -or
        $file.FullName -match 'material-color-utilities') {
        continue
    }

    $lines = @(Get-Content -Path $file.FullName)
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = [string]$lines[$i]
        $lineNum = $i + 1
        $trimmed = $line.Trim()

        # Skip comments
        if ($trimmed.StartsWith("//") -or $trimmed.StartsWith("/*") -or $trimmed.StartsWith("*") -or $trimmed.StartsWith("@*")) {
            continue
        }

        # Check Hex
        $hexMatches = $hexRegex.Matches($line)
        foreach ($m in $hexMatches) {
            $val = $m.Value
            # Filter false positives like hash routes, C# directives (#if, #region), or SVG url(#id)
            if ($trimmed.StartsWith("#if") -or $trimmed.StartsWith("#region") -or $trimmed.StartsWith("#endregion")) { continue }
            if ($line -match "url\($val\)") { continue }
            if ($line -match "href=`"$val`"") { continue }

            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "HardcodedHex"
                Snippet = $val
                Suggestion = Get-Suggestion $val $line
                Context = $trimmed
            }
        }

        # Check RGB
        $rgbMatches = $rgbRegex.Matches($line)
        foreach ($m in $rgbMatches) {
            $val = $m.Value
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "HardcodedRgb"
                Snippet = $val
                Suggestion = Get-Suggestion $val $line
                Context = $trimmed
            }
        }

        # Check Named CSS colors
        $namedMatches = $namedCssRegex.Matches($line)
        foreach ($m in $namedMatches) {
            $val = $m.Groups[1].Value
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "NamedColor"
                Snippet = $val
                Suggestion = Get-Suggestion $val $line
                Context = $trimmed
            }
        }

        # Check Tailwind color utility classes
        $twMatches = $tailwindColorRegex.Matches($line)
        foreach ($m in $twMatches) {
            $val = $m.Groups[1].Value
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "TailwindColorClass"
                Snippet = $val
                Suggestion = "Use Material 3 tokens (e.g. var(--md-sys-color-primary)) in companion .razor.css"
                Context = $trimmed
            }
        }
    }
}

# Group findings by type
$hexCount = ($findings | Where-Object { $_.Type -eq "HardcodedHex" }).Count
$rgbCount = ($findings | Where-Object { $_.Type -eq "HardcodedRgb" }).Count
$namedCount = ($findings | Where-Object { $_.Type -eq "NamedColor" }).Count
$twCount = ($findings | Where-Object { $_.Type -eq "TailwindColorClass" }).Count

Write-Host "`n📊 Color Audit Results:" -ForegroundColor Yellow
Write-Host "   - Hardcoded HEX colors: $hexCount" -ForegroundColor $(if ($hexCount -gt 0) { "Red" } else { "Green" })
Write-Host "   - Hardcoded RGB/RGBA: $rgbCount" -ForegroundColor $(if ($rgbCount -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - Hardcoded Named colors (white, black, etc.): $namedCount" -ForegroundColor $(if ($namedCount -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - Tailwind color classes: $twCount" -ForegroundColor $(if ($twCount -gt 0) { "Red" } else { "Green" })

if ($findings.Count -gt 0) {
    Write-Host "`n🔎 Sample Discrepancies (first 15):" -ForegroundColor Cyan
    $findings | Select-Object -First 15 | ForEach-Object {
        Write-Host "   - $($_.File):$($_.Line) [$($_.Type)] `"$($_.Snippet)`"" -ForegroundColor DarkYellow
        Write-Host "     💡 Suggestion: $($_.Suggestion)" -ForegroundColor DarkGreen
        Write-Host "     📝 Line: $($_.Context)" -ForegroundColor DarkGray
    }
    if ($findings.Count -gt 15) {
        Write-Host "   ... and $($findings.Count - 15) more occurrences." -ForegroundColor DarkGray
    }
} else {
    Write-Host "`n🎉 Clean design system! All colors strictly conform to Material 3 tokens." -ForegroundColor Green
}

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "  📋 Audit Finished: $($findings.Count) total issues found" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

if ($ExportJson) {
    $reportPath = Join-Path $repoRoot "theme-colors-audit-report.json"
    $findings | ConvertTo-Json -Depth 4 | Set-Content -Path $reportPath -Encoding UTF8
    Write-Host "Exported JSON report to: $reportPath" -ForegroundColor Green
}

