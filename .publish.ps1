<#
publish.ps1
-----------
Creates a release zip in:  <project>/publish/<guid>-<ver>.zip

Required inputs:
- icon:              <project>/_Assets/icon.png          -> else abort "icon missing"
- manifest template: <project>/_Assets/manifest.json     -> else abort "manifest missing"
- README:            <project>/README.md                 -> else abort "readme missing"
- LICENSE:           <project>/LICENSE                   -> else abort "license missing"

DLL rule:
- DLL must exist at: <project>/bin/Release/net480/<AssemblyName>.dll
  If AssemblyName is missing or equals '$(ProjectName)', ProjectName is used as fallback.

Zip rules:
- If zip already exists: abort "vergessen version zu updaten?"
- Optional extra payload: <project>/_Assets/BepInEx/ (copied into zip root first, if present)
- DLL is copied into: <zip>/BepInEx/plugins/<guid>/<AssemblyName>.dll
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Fail([string]$message) { throw $message }

# --- Project root (folder where this script lives) ---
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# --- Locate csproj (first one under project root) ---
$csproj = Get-ChildItem -Path $projectRoot -Filter *.csproj -Recurse | Select-Object -First 1
if (-not $csproj) { Fail "no csproj found" }

# --- Read properties from SDK-style csproj safely (works with StrictMode) ---
[xml]$projXml = Get-Content $csproj.FullName
$propertyGroups = $projXml.Project.PropertyGroup

function Get-Prop([string]$name) {
    foreach ($pg in $propertyGroups) {
        foreach ($child in $pg.ChildNodes) {
            if ($child.NodeType -eq [System.Xml.XmlNodeType]::Element -and $child.Name -eq $name) {
                $txt = $child.InnerText
                if (-not [string]::IsNullOrWhiteSpace($txt)) { return $txt.Trim() }
            }
        }
    }
    return $null
}

# --- Required csproj props ---
$guid = Get-Prop "PluginGUID"
if (-not $guid) { Fail "PluginGUID missing in csproj" }

# --- AssemblyName resolution strategy ---
# 1) Use <AssemblyName> if it's a real value
# 2) If AssemblyName is missing OR literally "$(ProjectName)", use <ProjectName>
# 3) If still missing, use csproj filename as last fallback
$assemblyName = Get-Prop "AssemblyName"
$projectName  = Get-Prop "ProjectName"

if ([string]::IsNullOrWhiteSpace($assemblyName) -or $assemblyName -eq '$(ProjectName)') {
    if (-not [string]::IsNullOrWhiteSpace($projectName)) {
        $assemblyName = $projectName
    }
}

if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    $assemblyName = [IO.Path]::GetFileNameWithoutExtension($csproj.Name)
}

$maj = Get-Prop "VersionMajor"
$min = Get-Prop "VersionMinor"
$pat = Get-Prop "VersionPatch"
$suf = Get-Prop "VersionSuffix"

if ($maj -eq $null -or $min -eq $null -or $pat -eq $null) {
    Fail "VersionMajor/VersionMinor/VersionPatch missing in csproj"
}

# --- Build version string (SemVer). VersionSuffix may be empty. ---
$ver = "$maj.$min.$pat"
if (-not [string]::IsNullOrWhiteSpace($suf)) { $ver = "$ver-$suf" }

Write-Host "Project:      $($csproj.Name)"
Write-Host "Plugin GUID:  $guid"
Write-Host "ProjectName:  $projectName"
Write-Host "AssemblyName: $assemblyName"
Write-Host "Version:      $ver"
Write-Host ""

# --- Required files (hard aborts) ---
$assetsDir      = Join-Path $projectRoot "_Assets"
$iconPath       = Join-Path $assetsDir "icon.png"
$manifestTpl    = Join-Path $assetsDir "manifest.json"
$readmePath     = Join-Path $projectRoot "README.md"
$changelogPath  = Join-Path $projectRoot "CHANGELOG.txt"
$licensePath    = Join-Path $projectRoot "LICENSE"

if (-not (Test-Path $iconPath))       { Fail "icon missing" }
if (-not (Test-Path $manifestTpl))    { Fail "manifest missing" }
if (-not (Test-Path $readmePath))     { Fail "readme missing" }
if (-not (Test-Path $changelogPath))  { Fail "changelog missing" }
if (-not (Test-Path $licensePath))    { Fail "license missing" }

# --- DLL location (hard rule) ---
$dllPath = Join-Path $projectRoot ("bin\Release\net480\{0}.dll" -f $assemblyName)
if (-not (Test-Path $dllPath)) { Fail ("dll missing at: " + $dllPath) }

# --- Output zip path ---
$publishDir = Join-Path $projectRoot ".publish"
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

$zipPath = Join-Path $publishDir ("{0}-{1}.zip" -f $guid, $ver)
if (Test-Path $zipPath) { Fail "vergessen version zu updaten?" }

# --- Create staging folder in temp ---
$tempBase = [System.IO.Path]::GetTempPath()
$tempName = "htc_pack_" + [System.Guid]::NewGuid().ToString("N")
$distRoot = Join-Path $tempBase $tempName
New-Item -ItemType Directory -Path $distRoot -Force | Out-Null

# --- Optional: copy _Assets/BepInEx into dist root (if it exists) ---
$optionalBepInEx = Join-Path $assetsDir "BepInEx"
if (Test-Path $optionalBepInEx) {
    Write-Host "Including optional assets folder: _Assets/BepInEx"
    Copy-Item $optionalBepInEx -Destination (Join-Path $distRoot "BepInEx") -Recurse -Force
}

# --- Always copy DLL into BepInEx/plugins/<guid>/<AssemblyName>.dll ---
$pluginOutDir = Join-Path $distRoot ("BepInEx\plugins\{0}" -f $guid)
New-Item -ItemType Directory -Path $pluginOutDir -Force | Out-Null
Copy-Item $dllPath -Destination (Join-Path $pluginOutDir ("{0}.dll" -f $assemblyName)) -Force

# --- Copy meta files to zip root ---
Copy-Item $iconPath       -Destination (Join-Path $distRoot "icon.png") -Force
Copy-Item $readmePath     -Destination (Join-Path $distRoot "README.md") -Force
Copy-Item $changelogPath  -Destination (Join-Path $distRoot "CHANGELOG.txt") -Force
Copy-Item $licensePath    -Destination (Join-Path $distRoot "LICENSE") -Force

# --- Load manifest template, patch version_number, write to dist root ---
$manifest = Get-Content $manifestTpl -Raw | ConvertFrom-Json
$manifest.version_number = $ver
($manifest | ConvertTo-Json -Depth 10) | Set-Content -Path (Join-Path $distRoot "manifest.json") -Encoding UTF8

# --- Create the zip ---
Compress-Archive -Path (Join-Path $distRoot "*") -DestinationPath $zipPath

# --- Cleanup temp folder ---
Remove-Item $distRoot -Recurse -Force

Write-Host ""
Write-Host "Done."
Write-Host "Created zip: $zipPath"
