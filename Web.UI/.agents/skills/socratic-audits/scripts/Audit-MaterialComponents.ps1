[CmdletBinding()]
param(
    [string]$RootPath = "$PSScriptRoot\..\..\..\..",
    [switch]$ExportJson = $false
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Normalize root path
$repoRoot = [System.IO.Path]::GetFullPath($RootPath)
$frontendDir = Join-Path $repoRoot "src\Frontend"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  🧩 Socratic Material.Web Components Auditor" -ForegroundColor Cyan
Write-Host "  Frontend: $frontendDir" -ForegroundColor DarkGray
Write-Host "==================================================" -ForegroundColor Cyan

if (-not (Test-Path $frontendDir)) {
    Write-Error "Frontend directory not found at: $frontendDir"
    return
}

# Regex patterns
$nativeButtonRegex = [regex]'<button\b[^>]*>'
$nativeInputRegex = [regex]'<input\b[^>]*>'
$nativeSelectRegex = [regex]'<select\b[^>]*>'
$nativeDialogRegex = [regex]'<dialog\b[^>]*>'
$nativeHrRegex = [regex]'<hr\b[^>]*>'
$thirdPartyRegex = [regex]'<(Fluent|Mud|Ant)[A-Z]\w+\b[^>]*>'
$rawMdElementRegex = [regex]'<md-(?:filled-button|outlined-button|text-button|elevated-button|tonal-button|outlined-text-field|filled-text-field|checkbox|switch|radio|outlined-select|filled-select)\b[^>]*>'

function Get-InputCategory([string]$inputTag) {
    if ($inputTag -match 'type=["'']hidden["'']') { return $null } # Safe to ignore
    if ($inputTag -match 'type=["'']checkbox["'']') {
        return @{ Type = "NativeCheckbox"; Suggestion = "<Checkbox> or <Switch>" }
    }
    if ($inputTag -match 'type=["'']radio["'']') {
        return @{ Type = "NativeRadio"; Suggestion = "<Radio>" }
    }
    if ($inputTag -match 'type=["'']range["'']') {
        return @{ Type = "NativeRange"; Suggestion = "<Slider> or <RangeSlider>" }
    }
    if ($inputTag -match 'type=["'']file["'']') {
        return @{ Type = "NativeFileInput"; Suggestion = "<FilePicker>" }
    }
    return @{ Type = "NativeTextInput"; Suggestion = "<TextField @bind-Value=""..."" Label=""..."" />" }
}

$files = Get-ChildItem -Path $frontendDir -Filter "*.razor" -Recurse -File

$findings = @()

foreach ($file in $files) {
    # Skip obj, bin, and Material.Web internal definitions
    if ($file.FullName -match '[\\/](obj|bin)[\\/]' -or 
        $file.FullName -match '[\\/]Material\.Web[\\/]') {
        continue
    }

    $lines = @(Get-Content -Path $file.FullName)
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = [string]$lines[$i]
        $lineNum = $i + 1
        $trimmed = $line.Trim()

        # Skip comments
        if ($trimmed.StartsWith("@*") -or $trimmed.StartsWith("//") -or $trimmed.StartsWith("/*")) {
            continue
        }

        # 1. Check native <button>
        $buttonMatches = $nativeButtonRegex.Matches($line)
        foreach ($m in $buttonMatches) {
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "NativeButton"
                Snippet = $m.Value
                Suggestion = "Replace with <Button Type=""filled|outlined|text""> or <IconButton>"
                Context = $trimmed
            }
        }

        # 2. Check native <input>
        $inputMatches = $nativeInputRegex.Matches($line)
        foreach ($m in $inputMatches) {
            $cat = Get-InputCategory $m.Value
            if ($cat) {
                $findings += [pscustomobject]@{
                    File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                    Line = $lineNum
                    Type = $cat.Type
                    Snippet = $m.Value
                    Suggestion = $cat.Suggestion
                    Context = $trimmed
                }
            }
        }

        # 3. Check native <select>
        $selectMatches = $nativeSelectRegex.Matches($line)
        foreach ($m in $selectMatches) {
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "NativeSelect"
                Snippet = $m.Value
                Suggestion = "Replace with <Select Items=""..."" @bind-Value=""..."">"
                Context = $trimmed
            }
        }

        # 4. Check native <dialog>
        $dialogMatches = $nativeDialogRegex.Matches($line)
        foreach ($m in $dialogMatches) {
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "NativeDialog"
                Snippet = $m.Value
                Suggestion = "Replace with <MaterialDialog Open=""..."">"
                Context = $trimmed
            }
        }

        # 5. Check native <hr>
        $hrMatches = $nativeHrRegex.Matches($line)
        foreach ($m in $hrMatches) {
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "NativeDivider"
                Snippet = $m.Value
                Suggestion = "Replace with <Divider />"
                Context = $trimmed
            }
        }

        # 6. Check third-party component libraries
        $tpMatches = $thirdPartyRegex.Matches($line)
        foreach ($m in $tpMatches) {
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "ThirdPartyLibraryComponent"
                Snippet = $m.Value
                Suggestion = "Replace third-party UI component with Material.Web RCL component"
                Context = $trimmed
            }
        }

        # 7. Check raw web components without Blazor wrapper
        $rawMdMatches = $rawMdElementRegex.Matches($line)
        foreach ($m in $rawMdMatches) {
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Type = "RawWebComponent"
                Snippet = $m.Value
                Suggestion = "Consider using typed Blazor wrapper (<Button>, <TextField>, <Checkbox>, etc.) for data binding"
                Context = $trimmed
            }
        }
    }
}

# Group findings by category
$buttons = ($findings | Where-Object { $_.Type -eq "NativeButton" }).Count
$inputs = ($findings | Where-Object { $_.Type -match "Native.*Input|NativeCheckbox|NativeRadio|NativeRange" }).Count
$selects = ($findings | Where-Object { $_.Type -eq "NativeSelect" }).Count
$dividers = ($findings | Where-Object { $_.Type -eq "NativeDivider" }).Count
$dialogs = ($findings | Where-Object { $_.Type -eq "NativeDialog" }).Count
$thirdParty = ($findings | Where-Object { $_.Type -eq "ThirdPartyLibraryComponent" }).Count
$rawWebComp = ($findings | Where-Object { $_.Type -eq "RawWebComponent" }).Count

Write-Host "`n📊 Material.Web Adoption Findings:" -ForegroundColor Yellow
Write-Host "   - Native <button> elements: $buttons" -ForegroundColor $(if ($buttons -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - Native <input> elements: $inputs" -ForegroundColor $(if ($inputs -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - Native <select> elements: $selects" -ForegroundColor $(if ($selects -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - Native <hr> dividers: $dividers" -ForegroundColor $(if ($dividers -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - Native <dialog> elements: $dialogs" -ForegroundColor $(if ($dialogs -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - Third-party library elements (Fluent/Mud/Ant): $thirdParty" -ForegroundColor $(if ($thirdParty -gt 0) { "Red" } else { "Green" })
Write-Host "   - Raw <md-*> elements (without Blazor wrapper): $rawWebComp" -ForegroundColor $(if ($rawWebComp -gt 0) { "Cyan" } else { "Green" })

if ($findings.Count -gt 0) {
    Write-Host "`n🔎 Sample Discrepancies (first 15):" -ForegroundColor Cyan
    $findings | Select-Object -First 15 | ForEach-Object {
        Write-Host "   - $($_.File):$($_.Line) [$($_.Type)]" -ForegroundColor DarkYellow
        Write-Host "     🔍 Snippet: $($_.Snippet)" -ForegroundColor White
        Write-Host "     💡 Suggestion: $($_.Suggestion)" -ForegroundColor DarkGreen
    }
    if ($findings.Count -gt 15) {
        Write-Host "   ... and $($findings.Count - 15) more occurrences." -ForegroundColor DarkGray
    }
} else {
    Write-Host "`n🎉 Perfect! All UI elements strictly use Material.Web RCL components." -ForegroundColor Green
}

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "  📋 Audit Finished: $($findings.Count) total places to review" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

if ($ExportJson) {
    $reportPath = Join-Path $repoRoot "material-components-audit-report.json"
    $findings | ConvertTo-Json -Depth 4 | Set-Content -Path $reportPath -Encoding UTF8
    Write-Host "Exported JSON report to: $reportPath" -ForegroundColor Green
}

