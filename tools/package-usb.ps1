# Builds dist\IDEA-Unstuck-USB-v<version>.zip, laid out exactly as it sits on a
# flash drive after "Extract All": the three .cmd files, README.txt, logs\, files\.
# Run after: dotnet build IdeaUnstuck.sln -c Release
param([string]$Version = '0.3.0', [string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$bin = Join-Path $root ('IdeaUnstuck.AddIn\bin\' + $Configuration + '\net48')
$dist = Join-Path $root 'dist'
$stage = Join-Path $dist ('IDEA-Unstuck-USB-v' + $Version)
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Path (Join-Path $stage 'files\icons'), (Join-Path $stage 'logs') -Force | Out-Null
Copy-Item (Join-Path $root 'usb\*.cmd'), (Join-Path $root 'usb\README.txt') $stage
Copy-Item (Join-Path $root 'usb\logs\*') (Join-Path $stage 'logs')
foreach ($f in 'IdeaUnstuck.AddIn.dll', 'IdeaUnstuck.Core.dll') { Copy-Item (Join-Path $bin $f) (Join-Path $stage 'files') }
foreach ($f in 'Setup.ps1', 'Initialize-User.ps1', 'Uninstall.cmd', 'Check.cmd') { Copy-Item (Join-Path $root ('scripts\' + $f)) (Join-Path $stage 'files') }
Copy-Item (Join-Path $root 'assets\icon\unstuck.ico') (Join-Path $stage 'files')
foreach ($f in 'unstuck-20.png', 'unstuck-32.png', 'unstuck-40.png', 'unstuck-64.png', 'unstuck-96.png', 'unstuck-128.png', 'taskpane-16x18.bmp', 'addin-16.bmp') { Copy-Item (Join-Path $root ('assets\icon\' + $f)) (Join-Path $stage 'files\icons') }
# Nothing from SOLIDWORKS may ship.
$sw = Get-ChildItem $stage -Recurse -File | Where-Object { $_.Name -like 'SolidWorks.Interop*' }
if ($sw) { throw ('A SOLIDWORKS DLL is in the package: ' + ($sw.Name -join ', ')) }
$v = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $stage 'files\IdeaUnstuck.AddIn.dll')).Version.ToString(3)
if ($v -ne $Version) { throw ('Built add-in is ' + $v + ', expected ' + $Version) }
$zip = Join-Path $dist ('IDEA-Unstuck-USB-v' + $Version + '.zip')
if (Test-Path $zip) { Remove-Item $zip -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($stage, $zip, [IO.Compression.CompressionLevel]::Optimal, $false)
$hash = (Get-FileHash $zip -Algorithm SHA256).Hash
Set-Content -Path ($zip + '.sha256') -Value ($hash + '  ' + (Split-Path $zip -Leaf)) -Encoding ASCII
Write-Output ('Package: ' + $zip)
Write-Output ('SHA-256: ' + $hash)
Get-ChildItem $stage -Recurse -File | ForEach-Object { '  ' + $_.FullName.Substring($stage.Length + 1) + '  ' + $_.Length }
