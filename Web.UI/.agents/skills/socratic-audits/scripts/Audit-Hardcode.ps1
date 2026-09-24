[CmdletBinding()]
param(
    [string]$RootPath = "$PSScriptRoot\..\..\..\..",
    [switch]$ExportJson = $false
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Normalize root path
$repoRoot = [System.IO.Path]::GetFullPath($RootPath)
$srcDir = Join-Path $repoRoot "src"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  🔒 Socratic Hardcode & Security Auditor" -ForegroundColor Cyan
Write-Host "  Source: $srcDir" -ForegroundColor DarkGray
Write-Host "==================================================" -ForegroundColor Cyan

if (-not (Test-Path $srcDir)) {
    Write-Error "Source directory not found at: $srcDir"
    return
}

# 1. Regex patterns
# Secrets & sensitive keys
$secretRegex = [regex]'(?i)(?:api_?key|jwt_?secret|client_?secret|private_?key|password)\s*[:=]\s*["'']([^"''\s]{8,})["'']'
$bearerRegex = [regex]'Bearer\s+[A-Za-z0-9-_=]+\.[A-Za-z0-9-_=]+\.[A-Za-z0-9-_.+/=]+'

# Network & URLs
$localhostRegex = [regex]'https?://(?:localhost|127\.0\.0\.1|0\.0\.0\.0)(?::\d+)?(?:/[^\s"''\r\n]*)?'
$hardcodedDomainRegex = [regex]'https?://(?:[a-zA-Z0-9-]+\.)?socratic\.uz(?:/[^\s"''\r\n]*)?'

# Absolute filesystem paths (excludes escape sequences like \n, \r, \t)
$windowsPathRegex = [regex]'(?<![a-zA-Z0-9])[a-zA-Z]:\\(?!n|r|t|0|[\\/])[a-zA-Z0-9_.-]+(?:\\[a-zA-Z0-9_.-]+)+'
$unixHomePathRegex = [regex]'/(?:home|Users)/[^"''\s\r\n]{4,}'

# Raw GUIDs in production code
$guidRegex = [regex]'[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}'

$files = Get-ChildItem -Path $srcDir -Include "*.cs", "*.razor", "*.json" -Recurse -File

$findings = @()

foreach ($file in $files) {
    # Skip obj, bin, launchSettings, and test files (for GUIDs and mock secrets)
    if ($file.FullName -match '[\\/](obj|bin)[\\/]' -or 
        $file.FullName -match 'node_modules' -or
        $file.Name -eq 'launchSettings.json') {
        continue
    }

    $isTest = $file.FullName -match '\.Test[\\/]'
    $isAppSettings = $file.Name -match 'appsettings.*\.json'
    $isSeed = $file.FullName -match 'Seed'

    $lines = @(Get-Content -Path $file.FullName)
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = [string]$lines[$i]
        $lineNum = $i + 1
        $trimmed = $line.Trim()

        # Skip comments
        if ($trimmed.StartsWith("//") -or $trimmed.StartsWith("/*") -or $trimmed.StartsWith("*") -or $trimmed.StartsWith("@*")) {
            continue
        }

        # 1. Check for Secrets
        if (-not $isTest) {
            $secretMatches = $secretRegex.Matches($line)
            foreach ($m in $secretMatches) {
                $val = $m.Groups[1].Value
                # Exclude placeholders, environment variable names, schema names
                if ($val -match '^(TODO|YOUR_|CHANGE_|dummy|test|null|empty|<.*>|\$|\{)' -or $line -match 'Configuration\[' -or $line -match 'AddParameter') {
                    continue
                }
                $findings += [pscustomobject]@{
                    File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                    Line = $lineNum
                    Severity = "CRITICAL"
                    Category = "SecuritySecret"
                    Snippet = "$($m.Value.Substring(0, [Math]::Min($m.Value.Length, 35)))..."
                    Suggestion = "Move secret to .NET Aspire parameters or User Secrets / Environment variables"
                    Context = $trimmed
                }
            }

            $bearerMatches = $bearerRegex.Matches($line)
            foreach ($m in $bearerMatches) {
                $findings += [pscustomobject]@{
                    File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                    Line = $lineNum
                    Severity = "CRITICAL"
                    Category = "HardcodedJwt"
                    Snippet = "$($m.Value.Substring(0, [Math]::Min($m.Value.Length, 30)))..."
                    Suggestion = "Remove static JWT token and inject via auth context"
                    Context = $trimmed
                }
            }
        }

        # 2. Check for Localhost / IP addresses
        if (-not $isTest -and -not $isAppSettings) {
            $lhMatches = $localhostRegex.Matches($line)
            foreach ($m in $lhMatches) {
                $findings += [pscustomobject]@{
                    File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                    Line = $lineNum
                    Severity = "HIGH"
                    Category = "LocalhostUrl"
                    Snippet = $m.Value
                    Suggestion = "Use .NET Aspire Service Discovery (e.g. 'https+http://service-name') or IConfiguration"
                    Context = $trimmed
                }
            }
        }

        # 3. Check for Hardcoded Absolute Filesystem Paths
        $winMatches = $windowsPathRegex.Matches($line)
        foreach ($m in $winMatches) {
            $val = $m.Value
            # Filter non-path false positives
            if ($val -match '^[a-zA-Z]:\\$' -or $line -match 'xmlns') { continue }
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Severity = "HIGH"
                Category = "AbsoluteFilePath"
                Snippet = $val
                Suggestion = "Use AppContext.BaseDirectory or IWebHostEnvironment.ContentRootPath with Path.Combine"
                Context = $trimmed
            }
        }

        $unixMatches = $unixHomePathRegex.Matches($line)
        foreach ($m in $unixMatches) {
            $findings += [pscustomobject]@{
                File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                Line = $lineNum
                Severity = "HIGH"
                Category = "AbsoluteFilePath"
                Snippet = $m.Value
                Suggestion = "Use AppContext.BaseDirectory or relative path"
                Context = $trimmed
            }
        }

        # 4. Check for Hardcoded Domain in business logic
        if (-not $isTest -and -not ($file.Name -in @('App.razor', 'SeoHead.razor', 'Program.cs'))) {
            $domainMatches = $hardcodedDomainRegex.Matches($line)
            foreach ($m in $domainMatches) {
                # Skip xml namespace
                if ($line -match 'xmlns') { continue }
                $findings += [pscustomobject]@{
                    File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                    Line = $lineNum
                    Severity = "MEDIUM"
                    Category = "HardcodedProductionUrl"
                    Snippet = $m.Value
                    Suggestion = "Inject NavigationManager.BaseUri or IConfiguration['PublicUrl']"
                    Context = $trimmed
                }
            }
        }

        # 5. Check for Raw GUIDs in production C# code
        if (-not $isTest -and -not $isSeed -and $file.Extension -eq '.cs' -and -not $isAppSettings) {
            $guidMatches = $guidRegex.Matches($line)
            foreach ($m in $guidMatches) {
                if ($line -match 'ObjectIdExtension\.DemoId' -or $line -match 'Guid\.Empty') { continue }
                $findings += [pscustomobject]@{
                    File = $file.FullName.Replace($repoRoot, "").TrimStart("\/")
                    Line = $lineNum
                    Severity = "LOW"
                    Category = "MagicGuid"
                    Snippet = $m.Value
                    Suggestion = "Define as a named constant in SharedKernel or accept as method parameter"
                    Context = $trimmed
                }
            }
        }
    }
}

# Group findings by Severity
$criticals = ($findings | Where-Object { $_.Severity -eq "CRITICAL" }).Count
$highs = ($findings | Where-Object { $_.Severity -eq "HIGH" }).Count
$mediums = ($findings | Where-Object { $_.Severity -eq "MEDIUM" }).Count
$lows = ($findings | Where-Object { $_.Severity -eq "LOW" }).Count

Write-Host "`n📊 Hardcode Audit Findings:" -ForegroundColor Yellow
Write-Host "   - 🔴 CRITICAL (Secrets, Keys, Passwords): $criticals" -ForegroundColor $(if ($criticals -gt 0) { "Red" } else { "Green" })
Write-Host "   - 🟠 HIGH (Localhost URLs, Absolute Local Paths): $highs" -ForegroundColor $(if ($highs -gt 0) { "Red" } else { "Green" })
Write-Host "   - 🟡 MEDIUM (Hardcoded Production URLs): $mediums" -ForegroundColor $(if ($mediums -gt 0) { "Yellow" } else { "Green" })
Write-Host "   - ⚪ LOW (Magic GUIDs in logic): $lows" -ForegroundColor $(if ($lows -gt 0) { "Yellow" } else { "Green" })

if ($findings.Count -gt 0) {
    Write-Host "`n🔎 Sample Discrepancies (first 15):" -ForegroundColor Cyan
    $findings | Select-Object -First 15 | ForEach-Object {
        $sevColor = switch ($_.Severity) {
            "CRITICAL" { "Red" }
            "HIGH"     { "DarkRed" }
            "MEDIUM"   { "Yellow" }
            default    { "DarkGray" }
        }
        Write-Host "   - [$($_.Severity)] $($_.File):$($_.Line) ($($_.Category))" -ForegroundColor $sevColor
        Write-Host "     🔍 Snippet: $($_.Snippet)" -ForegroundColor White
        Write-Host "     💡 Suggestion: $($_.Suggestion)" -ForegroundColor DarkGreen
    }
    if ($findings.Count -gt 15) {
        Write-Host "   ... and $($findings.Count - 15) more occurrences." -ForegroundColor DarkGray
    }
} else {
    Write-Host "`n🎉 Clean codebase! Zero hardcoded secrets, localhost URLs, or absolute paths detected." -ForegroundColor Green
}

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "  📋 Audit Finished: $($findings.Count) total issues found" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

if ($ExportJson) {
    $reportPath = Join-Path $repoRoot "hardcode-audit-report.json"
    $findings | ConvertTo-Json -Depth 4 | Set-Content -Path $reportPath -Encoding UTF8
    Write-Host "Exported JSON report to: $reportPath" -ForegroundColor Green
}

