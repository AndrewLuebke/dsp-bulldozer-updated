[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryRoot 'artifacts'
}
$projectPath = Join-Path $repositoryRoot 'Bulldozer.csproj'
$manifestPath = Join-Path $repositoryRoot 'manifest.json'
$iconPath = Join-Path $repositoryRoot 'icon.png'
$readmePath = Join-Path $repositoryRoot 'README.md'
$changelogPath = Join-Path $repositoryRoot 'CHANGELOG.md'

foreach ($requiredPath in @($projectPath, $manifestPath, $iconPath, $readmePath, $changelogPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required release file is missing: $requiredPath"
    }
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
[xml]$project = Get-Content -LiteralPath $projectPath -Raw
$projectVersion = $project.Project.PropertyGroup.Version | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($projectVersion) -or $manifest.version_number -ne $projectVersion) {
    throw "manifest.json version '$($manifest.version_number)' does not match project version '$projectVersion'."
}

Add-Type -AssemblyName System.Drawing
$icon = [System.Drawing.Image]::FromFile($iconPath)
try {
    if ($icon.Width -ne 256 -or $icon.Height -ne 256 -or $icon.RawFormat.Guid -ne [System.Drawing.Imaging.ImageFormat]::Png.Guid) {
        throw 'icon.png must be a 256x256 PNG.'
    }
}
finally {
    $icon.Dispose()
}

$dotnetPath = (Get-Command dotnet -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Source)
if ([string]::IsNullOrWhiteSpace($dotnetPath)) {
    $installedDotnet = Join-Path ([Environment]::GetFolderPath('ProgramFiles')) 'dotnet\dotnet.exe'
    if (Test-Path -LiteralPath $installedDotnet -PathType Leaf) {
        $dotnetPath = $installedDotnet
    }
}
if ([string]::IsNullOrWhiteSpace($dotnetPath)) {
    throw 'A .NET SDK is required to build this project. Install one and retry.'
}

& $dotnetPath build $projectPath -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

$targetFramework = $project.Project.PropertyGroup.TargetFramework | Select-Object -First 1
$dllPath = Join-Path $repositoryRoot "bin\$Configuration\$targetFramework\Bulldozer.dll"
if (-not (Test-Path -LiteralPath $dllPath -PathType Leaf)) {
    throw "Build did not produce $dllPath"
}

$assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($dllPath).Version
$expectedAssemblyVersion = [Version]::Parse("$projectVersion.0")
if ($assemblyVersion -ne $expectedAssemblyVersion) {
    throw "Assembly version '$assemblyVersion' does not match expected '$expectedAssemblyVersion'."
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$stagingDirectory = Join-Path $OutputDirectory 'staging'
if (Test-Path -LiteralPath $stagingDirectory) {
    Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingDirectory | Out-Null

$releaseFiles = @{
    'Bulldozer.dll' = $dllPath
    'manifest.json' = $manifestPath
    'README.md' = $readmePath
    'CHANGELOG.md' = $changelogPath
    'icon.png' = $iconPath
}
foreach ($entry in $releaseFiles.GetEnumerator()) {
    Copy-Item -LiteralPath $entry.Value -Destination (Join-Path $stagingDirectory $entry.Key)
}

$packagePath = Join-Path $OutputDirectory ("Bulldozer-{0}.zip" -f $projectVersion)
if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}
Compress-Archive -Path (Join-Path $stagingDirectory '*') -DestinationPath $packagePath -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $actualEntries = @($archive.Entries | ForEach-Object FullName | Sort-Object)
    $expectedEntries = @($releaseFiles.Keys | Sort-Object)
    if (Compare-Object -ReferenceObject $expectedEntries -DifferenceObject $actualEntries) {
        throw "Package contents must be exactly: $($expectedEntries -join ', ')"
    }
}
finally {
    $archive.Dispose()
}

Get-Item -LiteralPath $packagePath | Select-Object FullName, Length, LastWriteTime
