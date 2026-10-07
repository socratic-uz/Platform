<#
.SYNOPSIS
    Updates all full-page screenshots across Desktop, Tablet, and Mobile viewports for Socratic pages.

.DESCRIPTION
    Captures complete full-page screenshots (including everything extending beyond the viewport)
    for all platform routes and stores them in Platform\Design\Screenshots\.

.PARAMETER Device
    Target device filter: "Desktop", "Tablet", "Mobile", or "All" (default: "All").

.PARAMETER Page
    Specific page slug to capture: "landing", "home", "chat", "signin", etc., or "all" (default: "all").

.PARAMETER BaseUrl
    Base URL of the running Socratic Web App (default: "https://localhost:6443").

.EXAMPLE
    .\Update-Screenshots.ps1
    .\Update-Screenshots.ps1 -Device Mobile
    .\Update-Screenshots.ps1 -Page chat
#>

param(
    [string]$Device = "all",
    [string]$Page = "all",
    [string]$BaseUrl = "https://localhost:6443"
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RunnerScript = Join-Path $ScriptDir "sync-3-devices-all-pages.js"

$cliArgs = @()
if ($Device -ne "all") {
    $cliArgs += "--device=$Device"
}
if ($Page -ne "all") {
    $cliArgs += "--page=$Page"
}

$env:BASE_URL = $BaseUrl

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   Socratic Automated Full-Page Screenshot Updater" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Base URL : $BaseUrl" -ForegroundColor Gray
Write-Host "Device   : $Device" -ForegroundColor Gray
Write-Host "Page     : $Page" -ForegroundColor Gray
Write-Host "Target   : $ScriptDir" -ForegroundColor Gray
Write-Host ""

node $RunnerScript @cliArgs
