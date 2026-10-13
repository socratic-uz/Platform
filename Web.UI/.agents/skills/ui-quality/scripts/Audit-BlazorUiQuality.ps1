[CmdletBinding()]
param(
    [string]$TargetFolder = ".",
    [string]$RootPath = "",
    [switch]$ExportJson = $false
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Resolve working paths dynamically
$searchRoots = @(
    if ($RootPath) { $RootPath },
    $PWD.Path,
    "$PSScriptRoot\..\..\..\..",
    "$PSScriptRoot\..\..\.."
)

$analyzerDll = $null
$analyzerProject = $null

foreach ($root in $searchRoots) {
    if (-not (Test-Path $root)) { continue }
    $fullRoot = [System.IO.Path]::GetFullPath($root)
    
    $candidates = @(
        (Join-Path $fullRoot "Platform\Tools\BlazorUiQuality.Analyzer\bin\Debug\net10.0\BlazorUiQuality.Analyzer.dll"),
        (Join-Path $fullRoot "src\Frontend\Platform\Tools\BlazorUiQuality.Analyzer\bin\Debug\net10.0\BlazorUiQuality.Analyzer.dll"),
        (Join-Path $fullRoot "src\Tools\BlazorUiQuality.Analyzer\bin\Debug\net10.0\BlazorUiQuality.Analyzer.dll")
    )
    foreach ($cand in $candidates) {
        if (Test-Path $cand) {
            $analyzerDll = $cand
            break
        }
    }
    
    $projectCandidates = @(
        (Join-Path $fullRoot "Platform\Tools\BlazorUiQuality.Analyzer\BlazorUiQuality.Analyzer.csproj"),
        (Join-Path $fullRoot "src\Frontend\Platform\Tools\BlazorUiQuality.Analyzer\BlazorUiQuality.Analyzer.csproj"),
        (Join-Path $fullRoot "src\Tools\BlazorUiQuality.Analyzer\BlazorUiQuality.Analyzer.csproj")
    )
    foreach ($proj in $projectCandidates) {
        if (Test-Path $proj) {
            $analyzerProject = $proj
            break
        }
    }
    if ($analyzerDll -or $analyzerProject) { break }
}

$targetDir = [System.IO.Path]::GetFullPath((Join-Path $PWD.Path $TargetFolder))
if (-not (Test-Path $targetDir)) {
    foreach ($root in $searchRoots) {
        $candidate = Join-Path $root $TargetFolder
        if (Test-Path $candidate) {
            $targetDir = [System.IO.Path]::GetFullPath($candidate)
            break
        }
    }
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  ✨ Socratic Blazor UI Quality & Token Auditor" -ForegroundColor Cyan
Write-Host "  Target: $targetDir" -ForegroundColor DarkGray
Write-Host "==================================================" -ForegroundColor Cyan

if (-not (Test-Path $targetDir)) {
    Write-Error "Target directory not found at: $targetDir"
    return
}

# Ensure analyzer is built if needed
if (-not $analyzerDll -or -not (Test-Path $analyzerDll)) {
    if ($analyzerProject -and (Test-Path $analyzerProject)) {
        Write-Host "Building BlazorUiQuality.Analyzer..." -ForegroundColor Yellow
        dotnet build $analyzerProject -c Debug | Out-Null
        $analyzerDll = Join-Path (Split-Path $analyzerProject -Parent) "bin\Debug\net10.0\BlazorUiQuality.Analyzer.dll"
    }
}

if (-not $analyzerDll -or -not (Test-Path $analyzerDll)) {
    Write-Error "BlazorUiQuality.Analyzer.dll not found. Please build Platform/Tools/BlazorUiQuality.Analyzer."
    return
}

$formatArg = if ($ExportJson) { "json" } else { "human" }
$output = & dotnet $analyzerDll --source-root $targetDir --format $formatArg

if ($ExportJson) {
    $jsonPath = Join-Path $PSScriptRoot "ui-quality-report.json"
    $output | Out-File -FilePath $jsonPath -Encoding utf8
    Write-Host "Report exported to: $jsonPath" -ForegroundColor Green
} else {
    Write-Host $output
}
