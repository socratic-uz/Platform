[CmdletBinding()]
param(
    [string]$RootPath = "$PSScriptRoot\..\..\..\..",
    [switch]$CheckRazor = $true,
    [switch]$ExportJson = $false
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Resolve repository or target root dynamically
$repoRoot = [System.IO.Path]::GetFullPath($RootPath)

$candidates = @(
    (Join-Path $repoRoot "Platform\Shared\DesignSystem\Layout\Resources"),
    (Join-Path $repoRoot "src\Frontend\Platform\Shared\DesignSystem\Layout\Resources"),
    (Join-Path $repoRoot "Shared\DesignSystem\Layout\Resources"),
    (Join-Path $repoRoot "Layout\Resources"),
    (Join-Path $repoRoot "Resources")
)

$resourcesDir = $null
foreach ($c in $candidates) {
    if (Test-Path (Join-Path $c "ResourceRu.resx")) {
        $resourcesDir = $c
        break
    }
}

if (-not $resourcesDir) {
    $found = Get-ChildItem -Path $repoRoot -Filter "ResourceRu.resx" -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $resourcesDir = $found.DirectoryName
    } else {
        $frontendPlatform = "c:\Users\owner\source\repos\Socratic\src\Frontend\Platform\Shared\DesignSystem\Layout\Resources"
        if (Test-Path (Join-Path $frontendPlatform "ResourceRu.resx")) {
            $resourcesDir = $frontendPlatform
        }
    }
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  🔍 Socratic Localization (i18n) Auditor" -ForegroundColor Cyan
Write-Host "  Root: $repoRoot" -ForegroundColor DarkGray
Write-Host "  Resources: $resourcesDir" -ForegroundColor DarkGray
Write-Host "==================================================" -ForegroundColor Cyan

if (-not $resourcesDir -or -not (Test-Path $resourcesDir)) {
    Write-Error "Resources directory not found at: $resourcesDir"
    return
}

# 1. Load and parse .resx files
function Get-ResxKeys([string]$filePath) {
    if (-not (Test-Path $filePath)) { return @{} }
    [xml]$xml = Get-Content $filePath -Raw -Encoding UTF8
    $dict = [ordered]@{}
    foreach ($node in $xml.SelectNodes("//data")) {
        $keyName = $node.GetAttribute("name")
        $valNode = $node.SelectSingleNode("value")
        $valText = if ($valNode) { $valNode.InnerText } else { "" }
        if (-not [string]::IsNullOrWhiteSpace($keyName)) {
            $dict[$keyName] = $valText
        }
    }
    return $dict
}

$ruPath = Join-Path $resourcesDir "ResourceRu.resx"
$uzPath = Join-Path $resourcesDir "ResourceUz.resx"
$enPath = Join-Path $resourcesDir "ResourceEn.resx"

$ruKeys = Get-ResxKeys $ruPath
$uzKeys = Get-ResxKeys $uzPath
$enKeys = Get-ResxKeys $enPath

Write-Host "`n📊 RESX Key Summary:" -ForegroundColor Yellow
Write-Host "   - ResourceRu.resx: $($ruKeys.Count) keys"
Write-Host "   - ResourceUz.resx: $($uzKeys.Count) keys"
Write-Host "   - ResourceEn.resx: $($enKeys.Count) keys"

$allKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$ruKeys.Keys | ForEach-Object { [void]$allKeys.Add($_) }
$uzKeys.Keys | ForEach-Object { [void]$allKeys.Add($_) }
$enKeys.Keys | ForEach-Object { [void]$allKeys.Add($_) }

# 2. Check Key Asymmetry and Empty Values
$missingInUz = @()
$missingInEn = @()
$missingInRu = @()
$emptyKeys = @()

foreach ($key in $allKeys) {
    $inRu = $ruKeys.Contains($key)
    $inUz = $uzKeys.Contains($key)
    $inEn = $enKeys.Contains($key)

    if (-not $inUz) { $missingInUz += $key }
    if (-not $inEn) { $missingInEn += $key }
    if (-not $inRu) { $missingInRu += $key }

    if ($inRu -and [string]::IsNullOrWhiteSpace($ruKeys[$key])) {
        $emptyKeys += [pscustomobject]@{ Key = $key; File = "ResourceRu.resx" }
    }
    if ($inUz -and [string]::IsNullOrWhiteSpace($uzKeys[$key])) {
        $emptyKeys += [pscustomobject]@{ Key = $key; File = "ResourceUz.resx" }
    }
    if ($inEn -and [string]::IsNullOrWhiteSpace($enKeys[$key])) {
        $emptyKeys += [pscustomobject]@{ Key = $key; File = "ResourceEn.resx" }
    }
}

Write-Host "`n⚠️ RESX Discrepancies:" -ForegroundColor Yellow
if ($missingInUz.Count -gt 0) {
    Write-Host "   ❌ Missing in ResourceUz.resx: $($missingInUz.Count) keys" -ForegroundColor Red
    $missingInUz | Select-Object -First 10 | ForEach-Object { Write-Host "      - $_" -ForegroundColor DarkRed }
    if ($missingInUz.Count -gt 10) { Write-Host "      ... and $($missingInUz.Count - 10) more" -ForegroundColor DarkGray }
} else {
    Write-Host "   ✅ ResourceUz.resx has all keys" -ForegroundColor Green
}

if ($missingInEn.Count -gt 0) {
    Write-Host "   ❌ Missing in ResourceEn.resx: $($missingInEn.Count) keys" -ForegroundColor Red
    $missingInEn | Select-Object -First 10 | ForEach-Object { Write-Host "      - $_" -ForegroundColor DarkRed }
    if ($missingInEn.Count -gt 10) { Write-Host "      ... and $($missingInEn.Count - 10) more" -ForegroundColor DarkGray }
} else {
    Write-Host "   ✅ ResourceEn.resx has all keys" -ForegroundColor Green
}

if ($missingInRu.Count -gt 0) {
    Write-Host "   ⚠️ Missing in ResourceRu.resx: $($missingInRu.Count) keys" -ForegroundColor Yellow
    $missingInRu | ForEach-Object { Write-Host "      - $_" -ForegroundColor DarkYellow }
}

if ($emptyKeys.Count -gt 0) {
    Write-Host "   ⚠️ Keys with empty values: $($emptyKeys.Count)" -ForegroundColor Yellow
    $emptyKeys | Select-Object -First 5 | ForEach-Object { Write-Host "      - $($_.File) -> $($_.Key)" -ForegroundColor DarkYellow }
}

# 3. Scan .razor files for hardcoded strings
$hardcodedMatches = @()

if ($CheckRazor) {
    Write-Host "`n🔎 Scanning .razor files for hardcoded text..." -ForegroundColor Yellow
    $scanDir = if (Test-Path (Join-Path $repoRoot "src\Frontend")) {
        Join-Path $repoRoot "src\Frontend"
    } elseif (Test-Path (Join-Path $repoRoot "src")) {
        Join-Path $repoRoot "src"
    } else {
        $repoRoot
    }
    $razorFiles = Get-ChildItem -Path $scanDir -Filter "*.razor" -Recurse -File

    $cyrillicMarkupRegex = [regex]'>([^<@\r\n]*[\u0400-\u04FF]+[^<@]*)<'
    $cyrillicAttrRegex = [regex]'(?:Label|Title|Placeholder|Text|HelperText|Tooltip)="([^"@\r\n]*[\u0400-\u04FF]+[^"@\r\n]*)"'

    foreach ($file in $razorFiles) {
        # Skip obj/bin/.git/dist directories
        if ($file.FullName -match '[\\/](obj|bin|\.git|dist)[\\/]') { continue }

        $lines = @(Get-Content -Path $file.FullName)
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = [string]$lines[$i]
            $lineNum = $i + 1

            # Skip comments or code-behind blocks if on single line
            $trimmed = $line.Trim()
            if ($trimmed.StartsWith("@*") -or $trimmed.StartsWith("//") -or $trimmed.StartsWith("/*")) {
                continue
            }

            # Match tag contents
            $markupMatch = $cyrillicMarkupRegex.Matches($line)
            foreach ($m in $markupMatch) {
                $text = $m.Groups[1].Value.Trim()
                if ($text.Length -gt 1 -and -not ($text.StartsWith("@"))) {
                    $hardcodedMatches += [pscustomobject]@{
                        File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                        Line = $lineNum
                        Type = "TagText"
                        Snippet = $text
                    }
                }
            }

            # Match UI attributes
            $attrMatch = $cyrillicAttrRegex.Matches($line)
            foreach ($m in $attrMatch) {
                $text = $m.Groups[1].Value.Trim()
                if ($text.Length -gt 1 -and -not ($text.StartsWith("@"))) {
                    $hardcodedMatches += [pscustomobject]@{
                        File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                        Line = $lineNum
                        Type = "Attribute"
                        Snippet = $text
                    }
                }
            }
        }
    }

    if ($hardcodedMatches.Count -gt 0) {
        Write-Host "   ❌ Found $($hardcodedMatches.Count) hardcoded Cyrillic string(s) in Razor markup:" -ForegroundColor Red
        $hardcodedMatches | Select-Object -First 15 | ForEach-Object {
            Write-Host "      $($_.File):$($_.Line) [$($_.Type)] `"$($_.Snippet)`"" -ForegroundColor DarkCyan
        }
        if ($hardcodedMatches.Count -gt 15) {
            Write-Host "      ... and $($hardcodedMatches.Count - 15) more occurrences." -ForegroundColor DarkGray
        }
    } else {
        Write-Host "   ✅ No hardcoded Cyrillic strings detected in Razor markup!" -ForegroundColor Green
    }
}

# 4. Summary & Export
Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "  📋 Audit Finished" -ForegroundColor Cyan
Write-Host "  Missing in UZ: $($missingInUz.Count)"
Write-Host "  Missing in EN: $($missingInEn.Count)"
Write-Host "  Hardcoded Razor text: $($hardcodedMatches.Count)"
Write-Host "==================================================" -ForegroundColor Cyan

if ($ExportJson) {
    $report = @{
        Timestamp = (Get-Date).ToString("o")
        ResxSummary = @{
            RuKeys = $ruKeys.Count
            UzKeys = $uzKeys.Count
            EnKeys = $enKeys.Count
        }
        MissingInUz = $missingInUz
        MissingInEn = $missingInEn
        MissingInRu = $missingInRu
        EmptyKeys = $emptyKeys
        HardcodedRazor = $hardcodedMatches
    }
    $jsonPath = Join-Path $repoRoot "localization-audit-report.json"
    $report | ConvertTo-Json -Depth 5 | Set-Content -Path $jsonPath -Encoding UTF8
    Write-Host "Exported JSON report to: $jsonPath" -ForegroundColor Green
}

