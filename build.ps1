# Builds everything and the flash-drive ZIP. Needs a .NET SDK (8 or later).
# SOLIDWORKS is optional: without it the build uses the interop packages from
# nuget.org at build time only (see Directory.Build.props).
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & dotnet build IdeaUnstuck.sln -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    & (Join-Path $PSScriptRoot 'tools\package-usb.ps1')
} finally { Pop-Location }
