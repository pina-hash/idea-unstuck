$ErrorActionPreference = 'Stop'
try {
    $subkey = 'Software\SolidWorks\AddInsStartup\{962CE2F1-BF2C-46F3-A8A5-DC6081767E13}'
    $existing = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($subkey)
    if ($null -ne $existing) { $existing.Dispose() }
    else {
        $created = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($subkey)
        try { $created.SetValue('', 1, [Microsoft.Win32.RegistryValueKind]::DWord) }
        finally { $created.Dispose() }
    }
} catch {
    # A noninteractive logon task must never display an error dialog.
    try {
        $folder = Join-Path $env:LOCALAPPDATA 'IDEA\Unstuck'
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
        @{schema=1;utc=[DateTime]::UtcNow.ToString('o');eventName='startup_error';data=@{message=$_.Exception.Message}} | ConvertTo-Json -Compress | Add-Content -LiteralPath (Join-Path $folder ('log-'+(Get-Date -Format yyyyMMdd)+'.jsonl'))
    } catch {}
    exit 1
}
