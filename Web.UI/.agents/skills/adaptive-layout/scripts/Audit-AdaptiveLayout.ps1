param(
    [string]$Path = ".",
    [switch]$FailOnIssues
)

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Socratic Adaptive Layout & M3 Breakpoints Static Auditor" -ForegroundColor Cyan
Write-Host " Target Path: $Path" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$files = Get-ChildItem -Path $Path -Recurse -File -Include "*.razor", "*.razor.css", "*.css" |
    Where-Object { 
        $_.FullName -notmatch "node_modules" -and 
        $_.FullName -notmatch "\\bin\\" -and 
        $_.FullName -notmatch "\\obj\\" -and
        $_.FullName -notmatch "\\lib\\" -and
        $_.FullName -notmatch "\\dist\\" -and
        $_.FullName -notmatch "\\vendor\\" -and
        $_.FullName -notmatch "\\\.system_generated\\" -and
        $_.FullName -notmatch "\\scratch\\"
    }

$issueCount = 0
$issues = @()

foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $relPath = $file.FullName.Replace((Get-Location).Path + "\", "")
    
    for ($i = 0; $i -lt $content.Count; $i++) {
        $line = $content[$i]
        $lineNum = $i + 1

        # 1. Check for Flexbox violations
        if ($line -match "display\s*:\s*flex" -and $line -notmatch "/\*" -and $line -notmatch "//") {
            $issueCount++
            $issues += [PSCustomObject]@{
                File = $relPath
                Line = $lineNum
                Type = "Flexbox Violation"
                Detail = "Found 'display: flex'. Socratic strictly mandates CSS Grid ('display: grid')."
                Snippet = $line.Trim()
            }
        }

        # 2. Check for Non-M3 Breakpoints
        if ($line -match "@media[^{]*\(\s*(?:max|min)-width\s*:\s*([\d\.]+)px\s*\)") {
            $val = [double]$matches[1]
            $round = [Math]::Round($val)
            # M3 Standard Breakpoint classes: ~600 (Compact/Medium), ~840 (Medium/Expanded), ~1200 (Expanded/Large), ~1600 (Large/Extra-Large)
            $isStandard = ($round -in @(600, 840, 1200, 1600))
            if (-not $isStandard) {
                $issueCount++
                $issues += [PSCustomObject]@{
                    File = $relPath
                    Line = $lineNum
                    Type = "Non-M3 Breakpoint"
                    Detail = "Found non-standard breakpoint '${val}px'. Standard M3 Window Classes: 599.98px / 600px (Medium), 839.98px / 840px (Expanded), 1199.98px / 1200px (Large), 1599.98px / 1600px (Extra-Large)."
                    Snippet = $line.Trim()
                }
            }
        }

        # 3. Check for Dangerous Fixed Widths (> 500px without max-width)
        if ($line -notmatch "@media" -and $line -match "(?<!max-)(?:width|min-width)\s*:\s*([6-9]\d{2}|\d{4,})px" -and $line -notmatch "max-width" -and $line -notmatch "min\(" -and $line -notmatch "clamp\(") {
            # Skip if inside an Expanded/Large media query or if it's svg/canvas/chart width attribute
            if ($line -notmatch "viewBox" -and $line -notmatch "width=" -and $line -notmatch "transform" -and $line -notmatch "image") {
                $issueCount++
                $issues += [PSCustomObject]@{
                    File = $relPath
                    Line = $lineNum
                    Type = "Dangerous Fixed Width"
                    Detail = "Fixed width '$($matches[0])' risks horizontal overflow on Compact screens (< 600px). Use clamp() or min(100%, ...)."
                    Snippet = $line.Trim()
                }
            }
        }
    }
}

if ($issues.Count -eq 0) {
    Write-Host "`n[PASS] No Adaptive Layout, Breakpoint, or Flexbox violations found! Excellent job." -ForegroundColor Green
} else {
    Write-Host "`n[FAIL] Found $($issues.Count) Adaptive Layout issue(s):" -ForegroundColor Red
    foreach ($iss in $issues) {
        Write-Host " - $($iss.File):$($iss.Line) [$($iss.Type)]" -ForegroundColor Yellow
        Write-Host "   Detail: $($iss.Detail)" -ForegroundColor DarkGray
        Write-Host "   Snippet: $($iss.Snippet)" -ForegroundColor Gray
    }
    
    if ($FailOnIssues) {
        exit 1
    }
}

Write-Host "`nAudited $($files.Count) Razor and CSS files." -ForegroundColor Cyan
