[CmdletBinding()]
param(
    [string]$RootPath = "$PSScriptRoot\..\..\..\..",
    [string]$TargetFolder = "src\Frontend\Core\Shared",
    [switch]$ExportJson = $false
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$repoRoot = [System.IO.Path]::GetFullPath($RootPath)
$targetDir = Join-Path $repoRoot $TargetFolder
$analyzerProject = Join-Path $repoRoot "src\Tools\BlazorUiQuality.Analyzer\BlazorUiQuality.Analyzer.csproj"
$analyzerDll = Join-Path $repoRoot "src\Tools\BlazorUiQuality.Analyzer\bin\Debug\net10.0\BlazorUiQuality.Analyzer.dll"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  ✨ Socratic Blazor UI Quality & Token Auditor" -ForegroundColor Cyan
Write-Host "  Target: $targetDir" -ForegroundColor DarkGray
Write-Host "==================================================" -ForegroundColor Cyan

if (-not (Test-Path $targetDir)) {
    Write-Error "Target directory not found at: $targetDir"
    return
}

# Ensure analyzer is built
if (-not (Test-Path $analyzerDll)) {
    Write-Host "Building BlazorUiQuality.Analyzer..." -ForegroundColor Yellow
    dotnet build $analyzerProject -c Debug | Out-Null
    if (-not (Test-Path $analyzerDll)) {
        Write-Error "Failed to build BlazorUiQuality.Analyzer at $analyzerProject"
        return
    }
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
