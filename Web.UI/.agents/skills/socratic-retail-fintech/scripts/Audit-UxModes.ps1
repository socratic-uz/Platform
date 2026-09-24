[CmdletBinding()]
param (
    [string]$RepoRoot = "$PSScriptRoot/../../../..",
    [switch]$GenerateResxSnippets,
    [switch]$Detailed
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "   🔍 SOCRATIC 28 UNIVERSAL UX MODES AUDIT           " -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

$resolvedRoot = [System.IO.Path]::GetFullPath($RepoRoot)

# 1. Resolve ProductUxMode.cs
$uxCandidates = @(
    (Join-Path $resolvedRoot "src/Shared/SharedKernel/ValueObjects/ProductUxMode.cs"),
    (Join-Path $resolvedRoot "Shared/SharedKernel/ValueObjects/ProductUxMode.cs"),
    (Join-Path $resolvedRoot "src/Frontend/Retail/Commerce/Shared/SharedKernel/ValueObjects/ProductUxMode.cs"),
    (Join-Path $resolvedRoot "Retail/Commerce/Shared/SharedKernel/ValueObjects/ProductUxMode.cs")
)
$uxModeFile = $uxCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $uxModeFile) {
    $found = Get-ChildItem -Path $resolvedRoot -Filter "ProductUxMode.cs" -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { $uxModeFile = $found.FullName }
    else {
        $std = "c:/Users/owner/source/repos/Socratic/src/Shared/SharedKernel/ValueObjects/ProductUxMode.cs"
        if (Test-Path $std) { $uxModeFile = $std }
    }
}

# 2. Resolve ProductModuleRegistry.cs
$regCandidates = @(
    (Join-Path $resolvedRoot "src/Frontend/Retail/Commerce/Architecture/ProductModuleRegistry.cs"),
    (Join-Path $resolvedRoot "Retail/Commerce/Architecture/ProductModuleRegistry.cs"),
    (Join-Path $resolvedRoot "Architecture/ProductModuleRegistry.cs")
)
$registryFile = $regCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $registryFile) {
    $found = Get-ChildItem -Path $resolvedRoot -Filter "ProductModuleRegistry.cs" -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { $registryFile = $found.FullName }
    else {
        $std = "c:/Users/owner/source/repos/Socratic/src/Frontend/Retail/Commerce/Architecture/ProductModuleRegistry.cs"
        if (Test-Path $std) { $registryFile = $std }
    }
}

# 3. Resolve modulesDir
$modCandidates = @(
    (Join-Path $resolvedRoot "src/Frontend/Retail/Commerce/Modules"),
    (Join-Path $resolvedRoot "Retail/Commerce/Modules"),
    (Join-Path $resolvedRoot "Modules")
)
$modulesDir = $modCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $modulesDir) {
    $std = "c:/Users/owner/source/repos/Socratic/src/Frontend/Retail/Commerce/Modules"
    if (Test-Path $std) { $modulesDir = $std }
}

# 4. Resolve resourcesDir
$resCandidates = @(
    (Join-Path $resolvedRoot "src/Frontend/Platform/Shared/DesignSystem/Layout/Resources"),
    (Join-Path $resolvedRoot "Platform/Shared/DesignSystem/Layout/Resources"),
    (Join-Path $resolvedRoot "Shared/DesignSystem/Layout/Resources"),
    (Join-Path $resolvedRoot "Layout/Resources"),
    (Join-Path $resolvedRoot "Resources")
)
$resourcesDir = $resCandidates | Where-Object { Test-Path (Join-Path $_ "ResourceRu.resx") } | Select-Object -First 1
if (-not $resourcesDir) {
    $std = "c:/Users/owner/source/repos/Socratic/src/Frontend/Platform/Shared/DesignSystem/Layout/Resources"
    if (Test-Path $std) { $resourcesDir = $std }
}

if (-not $uxModeFile -or -not (Test-Path $uxModeFile)) {
    Write-Error "Could not find ProductUxMode.cs in $resolvedRoot or standard locations."
    exit 1
}

# 1. Parse all 28 UX Modes from ProductUxMode.cs
$uxContent = Get-Content $uxModeFile -Raw -Encoding UTF8
$modeRegex = [regex]'base\(nameof\((?<name>\w+)\),\s*(?<val>\d+),\s*"(?<icon>[^"]*)",\s*"(?<title>[^"]*)"\)'
$matches = $modeRegex.Matches($uxContent)

$modes = @()
foreach ($m in $matches) {
    $modes += [PSCustomObject]@{
        Name = $m.Groups['name'].Value
        Value = [int]$m.Groups['val'].Value
        Icon = $m.Groups['icon'].Value
        Title = $m.Groups['title'].Value
    }
}

$modes = $modes | Sort-Object Value

Write-Host "Parsed $($modes.Count) UX Modes from ProductUxMode.cs." -ForegroundColor Green

# 2. Parse ProductModuleRegistry.cs
$registryMappings = @{}
if (Test-Path $registryFile) {
    $regContent = Get-Content $registryFile -Raw -Encoding UTF8
    $regRegex = [regex]'\[ProductUxMode\.(?<name>\w+)\.Value\]\s*=\s*typeof\((?<comp>\w+)\)'
    foreach ($m in $regRegex.Matches($regContent)) {
        $registryMappings[$m.Groups['name'].Value] = $m.Groups['comp'].Value
    }
}

# 3. Locate Razor files in UI.Shared/Modules
$moduleFiles = if ($modulesDir -and (Test-Path $modulesDir)) {
    Get-ChildItem -Path $modulesDir -Filter "*.razor" -Recurse -File | Select-Object -ExpandProperty BaseName
} else {
    @()
}

# 4. Check Resx Localization
$ruResx = if ($resourcesDir) { Join-Path $resourcesDir "ResourceRu.resx" } else { "" }
$uzResx = if ($resourcesDir) { Join-Path $resourcesDir "ResourceUz.resx" } else { "" }
$enResx = if ($resourcesDir) { Join-Path $resourcesDir "ResourceEn.resx" } else { "" }

$ruKeys = @{}
$uzKeys = @{}
$enKeys = @{}

function Get-ResxKeys([string]$path) {
    $keys = @{}
    if (Test-Path $path) {
        [xml]$xml = Get-Content $path -Encoding UTF8
        foreach ($data in $xml.root.data) {
            if ($data.name) {
                $keys[$data.name] = $data.value
            }
        }
    }
    return $keys
}

if (Test-Path $ruResx) { $ruKeys = Get-ResxKeys $ruResx }
if (Test-Path $uzResx) { $uzKeys = Get-ResxKeys $uzResx }
if (Test-Path $enResx) { $enKeys = Get-ResxKeys $enResx }

# 5. Audit Each Mode
$auditResults = @()
$missingRegistration = 0
$missingComponent = 0
$missingRu = 0
$missingUz = 0
$missingEn = 0

foreach ($mode in $modes) {
    $isCatalog = ($mode.Value -eq 1)
    $mappedComponent = $registryMappings[$mode.Name]
    
    $regStatus = "OK"
    $compStatus = "OK"
    
    if ($isCatalog) {
        $mappedComponent = "(Default/Selector)"
        $regStatus = "OK (Default)"
        $compStatus = "OK"
    } else {
        if (-not $mappedComponent) {
            $regStatus = "MISSING"
            $missingRegistration++
        } else {
            if ($moduleFiles -contains $mappedComponent) {
                $compStatus = "OK"
            } else {
                $compStatus = "FILE NOT FOUND"
                $missingComponent++
            }
        }
    }

    $expectedKey = "UxMode_$($mode.Name)_Title"
    $hasRu = $ruKeys.ContainsKey($expectedKey)
    $hasUz = $uzKeys.ContainsKey($expectedKey)
    $hasEn = $enKeys.ContainsKey($expectedKey)

    if (-not $hasRu) { $missingRu++ }
    if (-not $hasUz) { $missingUz++ }
    if (-not $hasEn) { $missingEn++ }

    $locStatus = if ($hasRu -and $hasUz -and $hasEn) { "RU/UZ/EN OK" } else {
        $missingLangs = @()
        if (-not $hasRu) { $missingLangs += "RU" }
        if (-not $hasUz) { $missingLangs += "UZ" }
        if (-not $hasEn) { $missingLangs += "EN" }
        "Missing: $($missingLangs -join ', ')"
    }

    $auditResults += [PSCustomObject]@{
        Value = $mode.Value
        Mode = $mode.Name
        Icon = $mode.Icon
        Component = if ($mappedComponent) { $mappedComponent } else { "---" }
        Registry = $regStatus
        RazorFile = $compStatus
        Localization = $locStatus
        DefaultTitle = $mode.Title
    }
}

# Display Table
Write-Host "`nAUDIT MATRIX (28 Universal Product UX Modes):" -ForegroundColor Yellow
$auditResults | Format-Table -Property Value, Mode, Icon, Component, Registry, RazorFile, Localization -AutoSize

Write-Host "-----------------------------------------------------" -ForegroundColor DarkGray
Write-Host "AUDIT SUMMARY:" -ForegroundColor Cyan
Write-Host "Total Modes Defined:           $($modes.Count)/28" -ForegroundColor $(if ($modes.Count -eq 28) { "Green" } else { "Red" })
Write-Host "Registered in ModuleMap:       $(28 - $missingRegistration)/28" -ForegroundColor $(if ($missingRegistration -eq 0) { "Green" } else { "Yellow" })
Write-Host "Razor Modules Found on Disk:   $(28 - $missingComponent)/28" -ForegroundColor $(if ($missingComponent -eq 0) { "Green" } else { "Yellow" })
Write-Host "Localization Keys (RU):        $(28 - $missingRu)/28" -ForegroundColor $(if ($missingRu -eq 0) { "Green" } else { "Yellow" })
Write-Host "Localization Keys (UZ):        $(28 - $missingUz)/28" -ForegroundColor $(if ($missingUz -eq 0) { "Green" } else { "Yellow" })
Write-Host "Localization Keys (EN):        $(28 - $missingEn)/28" -ForegroundColor $(if ($missingEn -eq 0) { "Green" } else { "Yellow" })
Write-Host "=====================================================" -ForegroundColor Cyan

# 6. Generate Resx Snippet if requested
if ($GenerateResxSnippets -or ($missingRu -gt 0 -and $Detailed)) {
    Write-Host "`nGenerated .resx XML snippet for missing UX mode translations:" -ForegroundColor Magenta
    Write-Host "<!-- Insert into ResourceRu.resx / ResourceUz.resx / ResourceEn.resx -->" -ForegroundColor DarkGray
    foreach ($m in $modes) {
        Write-Host "  <data name=`"UxMode_$($m.Name)_Title`" xml:space=`"preserve`">"
        Write-Host "    <value>$($m.Title)</value>"
        Write-Host "  </data>"
    }
}
