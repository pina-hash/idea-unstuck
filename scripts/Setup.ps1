# IDEA Unstuck setup, v0.3. Runs under Windows PowerShell 5.1 (Windows 10 and 11).
#   -Mode Install    copy to Program Files, register for every Windows user, add Apps & features entry
#   -Mode Uninstall  remove all of it; students' parts and local help logs are kept
#   -Mode Check      read-only: what is installed, which SOLIDWORKS versions are covered
# -LogDir <folder>   also append one line per run to <folder>\<COMPUTERNAME>.txt (the flash drive)
# Ends with one large PASS or FAIL line and exit code 0 (PASS) or 1 (FAIL).
param(
    [Parameter(Mandatory = $true)][ValidateSet('Install', 'Uninstall', 'Check')][string]$Mode,
    [string]$LogDir = '',
    [string]$Source = ''
)
$ErrorActionPreference = 'Stop'
$Version = '0.3.0'
$Target = Join-Path $env:ProgramFiles 'IDEA\Unstuck'
$Guid = '{962CE2F1-BF2C-46F3-A8A5-DC6081767E13}'
$PaneClsid = '{73B7229D-3403-4476-B3D0-67B7A5781BA2}'
$RegAsm = Join-Path $env:SystemRoot 'Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe'
$RunName = 'IDEA Unstuck User Startup'
$UninstallKey = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\IDEA Unstuck'
$StartFolder = Join-Path $env:ProgramData 'Microsoft\Windows\Start Menu\Programs\IDEA Unstuck'
$Publisher = 'IDEA, Don Bosco Tech'
$Payload = @('IdeaUnstuck.AddIn.dll', 'IdeaUnstuck.Core.dll', 'Initialize-User.ps1', 'Setup.ps1', 'Uninstall.cmd', 'Check.cmd', 'unstuck.ico')
$Icons = @('unstuck-20.png', 'unstuck-32.png', 'unstuck-40.png', 'unstuck-64.png', 'unstuck-96.png', 'unstuck-128.png', 'taskpane-16x18.bmp', 'addin-16.bmp')
if (-not $Source) { $Source = $PSScriptRoot }
$Watch = [Diagnostics.Stopwatch]::StartNew()
$Notes = New-Object System.Collections.Generic.List[string]

function Say([string]$text) { Write-Host ('  ' + $text) }
function Banner([bool]$ok, [string]$reason) {
    $word = if ($ok) { 'PASS' } else { 'FAIL' }
    $color = if ($ok) { 'Green' } else { 'Red' }
    Write-Host ''
    Write-Host ('=' * 64) -ForegroundColor $color
    Write-Host ('   ' + $word + '   ' + $reason) -ForegroundColor $color
    Write-Host ('=' * 64) -ForegroundColor $color
    Write-Host ''
}
function Write-DriveLog([string]$result, [string]$reason) {
    if (-not $LogDir) { return }
    try {
        New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
        $sw = (Get-SolidWorksYears) -join ','
        if (-not $sw) { $sw = 'none' }
        $line = '{0} {1} {2} v{3} solidworks={4} windows={5} seconds={6:0.0} {7}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $Mode.ToUpper(), $result, $Version, $sw, (Get-WindowsName), $Watch.Elapsed.TotalSeconds, $reason
        Add-Content -LiteralPath (Join-Path $LogDir ($env:COMPUTERNAME + '.txt')) -Value $line -Encoding UTF8
    } catch { Say ('(Could not write the log on the drive: ' + $_.Exception.Message + ')') }
}
function Get-WindowsName {
    try { $v = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'; $b = [int]$v.CurrentBuild; if ($b -ge 22000) { 'Windows11-' + $b } else { 'Windows10-' + $b } } catch { 'Windows' }
}
function Test-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    return ([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}
function Test-Net48 {
    try { return ([int](Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -Name Release).Release) -ge 528040 } catch { return $false }
}
# SOLIDWORKS writes HKLM\SOFTWARE\SolidWorks\SOLIDWORKS <year>\Setup\SolidWorks Folder per installed version.
function Get-SolidWorksYears {
    $years = @()
    $root = 'HKLM:\SOFTWARE\SolidWorks'
    if (Test-Path $root) {
        foreach ($k in Get-ChildItem $root -ErrorAction SilentlyContinue) {
            if ($k.PSChildName -match '^SOLIDWORKS (\d{4})$') {
                $setup = Join-Path $k.PSPath 'Setup'
                $folder = $null
                try { $folder = (Get-ItemProperty $setup -Name 'SolidWorks Folder' -ErrorAction Stop).'SolidWorks Folder' } catch {}
                if ($folder -and (Test-Path (Join-Path $folder 'SLDWORKS.exe'))) { $years += $Matches[1] }
            }
        }
    }
    return @($years | Sort-Object -Unique)
}
function Get-InstalledVersion {
    $dll = Join-Path $Target 'IdeaUnstuck.AddIn.dll'
    if (-not (Test-Path -LiteralPath $dll)) { return $null }
    try { return [Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString(3) } catch { return 'unknown' }
}
function Set-StartupForLoadedUsers {
    # Everyone signed in right now (for example a student, while an admin approved
    # this install): turn the add-in on for them too, unless they already chose.
    foreach ($hive in Get-ChildItem 'Registry::HKEY_USERS' -ErrorAction SilentlyContinue) {
        if ($hive.PSChildName -notmatch '^S-1-5-21-[\d-]+$') { continue }
        $key = 'Registry::HKEY_USERS\' + $hive.PSChildName + '\Software\SolidWorks\AddInsStartup\' + $Guid
        if (-not (Test-Path -LiteralPath $key)) {
            New-Item -Path $key -Force | Out-Null
            New-ItemProperty -LiteralPath $key -Name '(default)' -PropertyType DWord -Value 1 -Force | Out-Null
        }
    }
}
function Remove-UserRegistration([string]$hive) {
    $key = 'Registry::HKEY_USERS\' + $hive + '\Software\SolidWorks\AddInsStartup\' + $Guid
    if (Test-Path -LiteralPath $key) { Remove-Item -LiteralPath $key -Recurse -Force }
}
function Stop-IfSolidWorksOpen {
    if (@(Get-Process SLDWORKS -ErrorAction SilentlyContinue).Count -gt 0) { throw 'SOLIDWORKS is open. Save your work, close SOLIDWORKS, then run this again.' }
}
function Unregister-Existing {
    $dll = Join-Path $Target 'IdeaUnstuck.AddIn.dll'
    if (Test-Path -LiteralPath $dll) {
        & $RegAsm $dll /unregister /nologo | Out-Null
        if ($LASTEXITCODE -ne 0) { Say ('Old registration removal returned ' + $LASTEXITCODE + '; continuing.') }
    }
}
function New-Shortcut([string]$path, [string]$targetPath, [string]$icon, [string]$description) {
    $shell = New-Object -ComObject WScript.Shell
    $s = $shell.CreateShortcut($path)
    $s.TargetPath = $targetPath; $s.IconLocation = $icon + ',0'; $s.Description = $description; $s.WorkingDirectory = Split-Path $targetPath
    $s.Save()
}

function Install {
    if (-not (Test-Admin)) { throw 'This needs administrator rights. Double-click Install IDEA Unstuck.cmd and choose Yes.' }
    if (-not (Test-Net48)) { throw '.NET Framework 4.8 is missing. Install it from Windows Update, then run this again.' }
    Stop-IfSolidWorksOpen
    foreach ($f in $Payload) { if (-not (Test-Path -LiteralPath (Join-Path $Source $f))) { throw ('Missing file ' + $f + '. Copy the whole folder from the ZIP again.') } }
    foreach ($f in $Icons) { if (-not (Test-Path -LiteralPath (Join-Path $Source ('icons\' + $f)))) { throw ('Missing file icons\' + $f + '. Copy the whole folder from the ZIP again.') } }
    Get-ChildItem -LiteralPath $Source -Recurse -File | Unblock-File -ErrorAction SilentlyContinue
    $before = Get-InstalledVersion
    if ($before) { Say ('Found IDEA Unstuck ' + $before + '. Upgrading it.') }
    $years = Get-SolidWorksYears
    if ($years.Count -eq 0) { $Notes.Add('no SOLIDWORKS found on this PC; installed anyway') | Out-Null; Say 'No SOLIDWORKS found. Installing anyway; it will load once SOLIDWORKS is installed.' }
    else { Say ('SOLIDWORKS found: ' + ($years -join ', ')) }
    Unregister-Existing
    New-Item -ItemType Directory -Path (Join-Path $Target 'icons') -Force | Out-Null
    foreach ($f in $Payload) { Copy-Item -LiteralPath (Join-Path $Source $f) -Destination $Target -Force }
    foreach ($f in $Icons) { Copy-Item -LiteralPath (Join-Path $Source ('icons\' + $f)) -Destination (Join-Path $Target 'icons') -Force }
    Get-ChildItem -LiteralPath $Target -Recurse -File | Unblock-File -ErrorAction SilentlyContinue
    & $RegAsm (Join-Path $Target 'IdeaUnstuck.AddIn.dll') /codebase /nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { throw ('Registration (RegAsm) failed with code ' + $LASTEXITCODE + '.') }
    # Every Windows user, including a student's first ever sign-in: this runs at each
    # sign-in and turns the add-in on unless that student already chose otherwise.
    $run = '"' + $env:SystemRoot + '\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File "' + $Target + '\Initialize-User.ps1"'
    New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name $RunName -PropertyType String -Value $run -Force | Out-Null
    & (Join-Path $Target 'Initialize-User.ps1')
    Set-StartupForLoadedUsers
    # Apps & features
    New-Item -Path $UninstallKey -Force | Out-Null
    $size = [int]((Get-ChildItem -LiteralPath $Target -Recurse -File | Measure-Object Length -Sum).Sum / 1KB)
    $values = @{ DisplayName = 'IDEA Unstuck'; DisplayVersion = $Version; Publisher = $Publisher; DisplayIcon = (Join-Path $Target 'unstuck.ico'); InstallLocation = $Target
                 UninstallString = ('"' + (Join-Path $Target 'Uninstall.cmd') + '"'); Comments = 'Offline SOLIDWORKS help for students.' }
    foreach ($k in $values.Keys) { New-ItemProperty -Path $UninstallKey -Name $k -PropertyType String -Value $values[$k] -Force | Out-Null }
    foreach ($k in @('NoModify', 'NoRepair')) { New-ItemProperty -Path $UninstallKey -Name $k -PropertyType DWord -Value 1 -Force | Out-Null }
    New-ItemProperty -Path $UninstallKey -Name 'EstimatedSize' -PropertyType DWord -Value $size -Force | Out-Null
    # Start menu: one shortcut, with the icon, to the read-only check.
    New-Item -ItemType Directory -Path $StartFolder -Force | Out-Null
    New-Shortcut (Join-Path $StartFolder 'Check IDEA Unstuck.lnk') (Join-Path $Target 'Check.cmd') (Join-Path $Target 'unstuck.ico') 'Shows whether IDEA Unstuck is installed and which SOLIDWORKS versions it covers.'
    # Verify what we just did.
    $problems = Get-Problems
    if ($problems.Count -gt 0) { throw ('Installed, but these checks failed: ' + ($problems -join '; ')) }
    $what = if ($before) { 'Upgraded ' + $before + ' to ' + $Version } else { 'Installed ' + $Version }
    $cover = if ($years.Count -gt 0) { 'for SOLIDWORKS ' + ($years -join ' and ') } else { '(no SOLIDWORKS found yet)' }
    return ($what + ' ' + $cover + '. Every Windows user gets it when SOLIDWORKS starts.')
}

function Get-Problems {
    $p = New-Object System.Collections.Generic.List[string]
    foreach ($f in $Payload) { if (-not (Test-Path -LiteralPath (Join-Path $Target $f))) { $p.Add('missing ' + $f) | Out-Null } }
    foreach ($f in $Icons) { if (-not (Test-Path -LiteralPath (Join-Path $Target ('icons\' + $f)))) { $p.Add('missing icons\' + $f) | Out-Null } }
    $addin = 'HKLM:\SOFTWARE\SolidWorks\Addins\' + $Guid
    if (-not (Test-Path $addin) -or (Get-Item $addin).GetValue('Title') -ne 'IDEA Unstuck') { $p.Add('SOLIDWORKS add-in entry missing') | Out-Null }
    foreach ($clsid in @($Guid, $PaneClsid)) {
        $inproc = 'Registry::HKEY_CLASSES_ROOT\CLSID\' + $clsid + '\InprocServer32'
        $code = $null
        try { $code = (Get-ItemProperty -LiteralPath $inproc -Name CodeBase -ErrorAction Stop).CodeBase } catch {}
        if (-not $code -or $code -notlike '*IDEA/Unstuck/IdeaUnstuck.AddIn.DLL') { $p.Add('COM registration for ' + $clsid + ' does not point at Program Files') | Out-Null }
    }
    try { $null = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name $RunName -ErrorAction Stop } catch { $p.Add('sign-in startup entry missing') | Out-Null }
    if (-not (Test-Path $UninstallKey)) { $p.Add('Apps & features entry missing') | Out-Null }
    return $p
}

function Uninstall {
    if (-not (Test-Admin)) { throw 'This needs administrator rights. Double-click Uninstall IDEA Unstuck.cmd and choose Yes.' }
    Stop-IfSolidWorksOpen
    $before = Get-InstalledVersion
    Unregister-Existing
    Remove-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name $RunName -ErrorAction SilentlyContinue
    foreach ($key in @(('HKLM:\SOFTWARE\SolidWorks\Addins\' + $Guid), ('HKCU:\Software\SolidWorks\AddInsStartup\' + $Guid), $UninstallKey)) { if (Test-Path -LiteralPath $key) { Remove-Item -LiteralPath $key -Recurse -Force } }
    foreach ($clsid in @($Guid, $PaneClsid)) { $k = 'Registry::HKEY_CLASSES_ROOT\CLSID\' + $clsid; if (Test-Path -LiteralPath $k) { Remove-Item -LiteralPath $k -Recurse -Force } }
    foreach ($progid in @('IdeaUnstuck.AddIn', 'IdeaUnstuck.Pane')) { $k = 'Registry::HKEY_CLASSES_ROOT\' + $progid; if (Test-Path -LiteralPath $k) { Remove-Item -LiteralPath $k -Recurse -Force } }
    foreach ($profile in Get-CimInstance Win32_UserProfile | Where-Object { -not $_.Special }) {
        if ($profile.Loaded) { Remove-UserRegistration $profile.SID; continue }
        $hiveFile = Join-Path $profile.LocalPath 'NTUSER.DAT'
        if (-not (Test-Path -LiteralPath $hiveFile)) { continue }
        $mount = 'IDEA_Unstuck_Uninstall_' + [Guid]::NewGuid().ToString('N')
        & reg.exe load ('HKU\' + $mount) $hiveFile 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { $Notes.Add('could not open the sign-in settings of ' + $profile.LocalPath) | Out-Null; continue }
        try { Remove-UserRegistration $mount } finally { [GC]::Collect(); [GC]::WaitForPendingFinalizers(); & reg.exe unload ('HKU\' + $mount) 2>&1 | Out-Null }
    }
    if (Test-Path -LiteralPath $StartFolder) { Remove-Item -LiteralPath $StartFolder -Recurse -Force }
    if (Test-Path -LiteralPath $Target) {
        $resolved = (Resolve-Path -LiteralPath $Target).ProviderPath
        if ($resolved.TrimEnd('\') -ne (Join-Path $env:ProgramFiles 'IDEA\Unstuck')) { throw 'Unexpected installation path. Cleanup stopped.' }
        Remove-Item -LiteralPath $resolved -Recurse -Force
        $parent = Join-Path $env:ProgramFiles 'IDEA'
        if ((Test-Path $parent) -and @(Get-ChildItem $parent).Count -eq 0) { Remove-Item $parent -Force }
    }
    $left = @()
    if (Test-Path -LiteralPath $Target) { $left += 'Program Files folder' }
    if (Test-Path ('HKLM:\SOFTWARE\SolidWorks\Addins\' + $Guid)) { $left += 'add-in entry' }
    if (Test-Path $UninstallKey) { $left += 'Apps & features entry' }
    if ($left.Count -gt 0) { throw ('Some parts could not be removed: ' + ($left -join ', ')) }
    $what = if ($before) { 'Removed IDEA Unstuck ' + $before } else { 'IDEA Unstuck was not installed; nothing left behind' }
    return ($what + '. Student parts and help logs were kept.')
}

function Check {
    $v = Get-InstalledVersion
    $years = Get-SolidWorksYears
    Say ('Installed version:   ' + $(if ($v) { $v } else { 'not installed' }))
    Say ('SOLIDWORKS on this PC: ' + $(if ($years.Count) { $years -join ', ' } else { 'none found' }))
    if (-not $v) { throw 'IDEA Unstuck is not installed on this PC.' }
    $problems = Get-Problems
    foreach ($f in $Payload + ($Icons | ForEach-Object { 'icons\' + $_ })) { Say ((('{0,-28}' -f $f)) + $(if (Test-Path -LiteralPath (Join-Path $Target $f)) { 'present' } else { 'MISSING' })) }
    $me = 'HKCU:\Software\SolidWorks\AddInsStartup\' + $Guid
    $mine = 'not set yet (it is set at your next Windows sign-in)'
    if (Test-Path $me) { $mine = if ((Get-Item $me).GetValue('') -eq 1) { 'on' } else { 'turned off in Tools > Add-Ins' } }
    Say ('Starts with SOLIDWORKS for you: ' + $mine)
    if (Get-Process SLDWORKS -ErrorAction SilentlyContinue) { Say 'SOLIDWORKS is running now. That is fine for this check.' }
    if ($problems.Count -gt 0) { throw ('Installed ' + $v + ', but: ' + ($problems -join '; ')) }
    if ($years.Count -eq 0) { $Notes.Add('no SOLIDWORKS found') | Out-Null }
    return ('IDEA Unstuck ' + $v + ' is installed' + $(if ($years.Count) { ' for SOLIDWORKS ' + ($years -join ' and ') } else { '. No SOLIDWORKS found on this PC.' }) + '.')
}

try {
    Write-Host ''
    Write-Host ('IDEA Unstuck ' + $Version + ' - ' + $Mode) -ForegroundColor Cyan
    $reason = switch ($Mode) { 'Install' { Install } 'Uninstall' { Uninstall } 'Check' { Check } }
    if ($Notes.Count) { $reason += ' Note: ' + ($Notes -join '; ') + '.' }
    Say ('Took {0:0.0} seconds.' -f $Watch.Elapsed.TotalSeconds)
    Banner $true $reason
    Write-DriveLog 'PASS' $reason
    exit 0
} catch {
    $msg = $_.Exception.Message
    Banner $false $msg
    Write-DriveLog 'FAIL' $msg
    exit 1
}
