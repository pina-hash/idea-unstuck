param([Parameter(Mandatory=$true)][ValidateSet('Install','Uninstall')][string]$Mode)
$ErrorActionPreference = 'Stop'
$target = 'C:\Program Files\IDEA\Unstuck'
$guid = '{962CE2F1-BF2C-46F3-A8A5-DC6081767E13}'
$regasm = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe'
$repo = Split-Path -Parent $PSScriptRoot
$runName = 'IDEA Unstuck User Startup'
function Assert-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    if (-not ([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Right-click this batch file and choose Run as administrator.' }
}
function Remove-UserRegistration([string]$hive) {
    $key = 'Registry::HKEY_USERS\'+$hive+'\Software\SolidWorks\AddInsStartup\'+$guid
    if (Test-Path -LiteralPath $key) { Remove-Item -LiteralPath $key -Recurse -Force }
}
try {
    Assert-Admin
    foreach ($swProcess in Get-Process SLDWORKS -ErrorAction SilentlyContinue) {
        if (@($swProcess.Modules | Where-Object { $_.FileName -like ($target+'\*') }).Count -gt 0) { throw 'Close the SOLIDWORKS instance using IDEA Unstuck before changing its installation.' }
    }
    if ($Mode -eq 'Install') {
        $source = Join-Path $repo 'release'
        if (-not (Test-Path -LiteralPath (Join-Path $source 'IdeaUnstuck.AddIn.dll'))) { $source = Join-Path $repo 'IdeaUnstuck.AddIn\bin\Release\net48' }
        foreach ($file in @('IdeaUnstuck.AddIn.dll','IdeaUnstuck.Core.dll')) { if (-not (Test-Path -LiteralPath (Join-Path $source $file))) { throw ('Missing '+$file+'. Run build.ps1 first, or use the release ZIP.') } }
        New-Item -ItemType Directory -Path $target -Force | Out-Null
        foreach ($file in @('IdeaUnstuck.AddIn.dll','IdeaUnstuck.Core.dll')) { Copy-Item -LiteralPath (Join-Path $source $file) -Destination $target -Force }
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Initialize-User.ps1') -Destination $target -Force
        & $regasm (Join-Path $target 'IdeaUnstuck.AddIn.dll') /codebase /nologo
        if ($LASTEXITCODE -ne 0) { throw ('RegAsm failed with code '+$LASTEXITCODE) }
        $run = '"'+$env:SystemRoot+'\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File "'+$target+'\Initialize-User.ps1"'
        New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name $runName -PropertyType String -Value $run -Force | Out-Null
        & (Join-Path $target 'Initialize-User.ps1')
        if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Current-user startup initialization failed.' }
        $key = Get-Item -LiteralPath ('HKLM:\SOFTWARE\SolidWorks\Addins\'+$guid)
        if ($key.GetValue('Title') -ne 'IDEA Unstuck') { throw 'Add-in registry verification failed.' }
        Write-Output 'Verified program files, COM registration, and per-user logon initialization.'
    } else {
        $dll = Join-Path $target 'IdeaUnstuck.AddIn.dll'
        if (Test-Path -LiteralPath $dll) { & $regasm $dll /unregister /nologo; if ($LASTEXITCODE -ne 0) { throw ('RegAsm removal failed with code '+$LASTEXITCODE) } }
        Remove-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name $runName -ErrorAction SilentlyContinue
        foreach ($key in @(('HKLM:\SOFTWARE\SolidWorks\Addins\'+$guid),('HKCU:\Software\SolidWorks\AddInsStartup\'+$guid))) { if (Test-Path -LiteralPath $key) { Remove-Item -LiteralPath $key -Recurse -Force } }
        foreach ($profile in Get-CimInstance Win32_UserProfile | Where-Object { -not $_.Special }) {
            if ($profile.Loaded) { Remove-UserRegistration $profile.SID; continue }
            $hiveFile = Join-Path $profile.LocalPath 'NTUSER.DAT'
            if (-not (Test-Path -LiteralPath $hiveFile)) { continue }
            $mount = 'IDEA_Unstuck_Uninstall_'+[Guid]::NewGuid().ToString('N')
            & reg.exe load ('HKU\'+$mount) $hiveFile | Out-Null
            if ($LASTEXITCODE -ne 0) { throw ('Could not clean startup registration for '+$profile.SID) }
            try { Remove-UserRegistration $mount } finally { [GC]::Collect(); [GC]::WaitForPendingFinalizers(); & reg.exe unload ('HKU\'+$mount) | Out-Null; if ($LASTEXITCODE -ne 0) { throw ('Could not unload registry hive '+$mount) } }
        }
        if (Test-Path -LiteralPath $target) {
            $resolved = (Resolve-Path -LiteralPath $target).ProviderPath
            if ($resolved.TrimEnd('\') -ne 'C:\Program Files\IDEA\Unstuck') { throw 'Unexpected installation path. Cleanup stopped.' }
            Remove-Item -LiteralPath $resolved -Recurse -Force
        }
        Write-Output 'Removed the add-in, COM registration, logon entry, and student startup entries.'
    }
    exit 0
} catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }
