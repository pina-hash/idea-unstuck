param(
    [Parameter(Mandatory=$true)][string]$InputFolder,
    [string]$OutputPath = (Join-Path $PWD 'teacher-summary.html')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$rows = [Collections.Generic.List[object]]::new()
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$bad = 0
function Read-Lines([IO.TextReader]$reader) {
    while ($null -ne ($line = $reader.ReadLine())) {
        if ([string]::IsNullOrWhiteSpace($line) -or -not $seen.Add($line)) { continue }
        try {
            $row = $line | ConvertFrom-Json
            $when = [DateTimeOffset]::Parse($row.utc, [Globalization.CultureInfo]::InvariantCulture)
            if (-not $row.session -or -not $row.eventName) { throw 'Missing event fields' }
            $rows.Add([pscustomobject]@{ utc=$when; session=[string]$row.session; eventName=[string]$row.eventName; data=$row.data })
        } catch { $script:bad++ }
    }
}
foreach ($file in Get-ChildItem -LiteralPath $InputFolder -File -Recurse | Sort-Object FullName) {
    if ($file.Extension -eq '.jsonl') {
        $reader=[IO.File]::OpenText($file.FullName)
        try { Read-Lines $reader } finally { $reader.Dispose() }
    } elseif ($file.Extension -eq '.zip') {
        $zip=[IO.Compression.ZipFile]::OpenRead($file.FullName)
        try { foreach($entry in $zip.Entries) { if ($entry.FullName.EndsWith('.jsonl',[StringComparison]::OrdinalIgnoreCase)) {
            $reader=[IO.StreamReader]::new($entry.Open())
            try { Read-Lines $reader } finally { $reader.Dispose() }
        } } } finally { $zip.Dispose() }
    }
}
$diagnoses=@{}; $buttons=@{}; $people=@{}; $users=@{}; $bodyTimes=[Collections.Generic.List[double]]::new()
function Initials([string]$name) {
    $parts=@($name -split '[^\p{L}\p{Nd}]+' | Where-Object { $_ })
    if ($parts.Count -eq 0) { return '?' }
    return (($parts | Select-Object -First 3 | ForEach-Object { $_.Substring(0,1).ToUpperInvariant() }) -join '')
}
foreach($r in $rows | Sort-Object utc) {
    if ($r.eventName -eq 'session_start') { $users[$r.session]=Initials ([string]$r.data.user) }
    if ($r.eventName -eq 'diagnosis_appeared' -or $r.eventName -eq 'diagnosis_cleared') {
        $id=[string]$r.data.id
        if (-not $id) { continue }
        # Dynamic feature names can identify student files; aggregate by diagnosis kind.
        $id=($id -split ':',2)[0]
        if (-not $diagnoses.ContainsKey($id)) { $diagnoses[$id]=@{count=0;seconds=0.0} }
        if ($r.eventName -eq 'diagnosis_appeared') { $diagnoses[$id].count++ }
        else { $diagnoses[$id].seconds += [Math]::Max(0,[double]$r.data.seconds) }
    }
    if ($r.eventName -eq 'button_pressed') { $key=[string]$r.data.action; if($key){$buttons[$key]++} }
    if ($r.eventName -eq 'stuck_triggered') { $key=if($users.ContainsKey($r.session)){$users[$r.session]}else{'?'}; $people[$key]++ }
    if ($r.eventName -eq 'first_body' -and -not $r.data.existingBody) { $bodyTimes.Add([Math]::Max(0,[double]$r.data.seconds)) }
}
$rank=@($diagnoses.Keys | ForEach-Object {[pscustomobject]@{id=$_;count=$diagnoses[$_].count;minutes=[Math]::Round($diagnoses[$_].seconds/60,2)}})
$ordered=@($bodyTimes | Sort-Object)
$median=$null
if($ordered.Count){$middle=[int][Math]::Floor($ordered.Count/2); $median=if($ordered.Count%2){$ordered[$middle]}else{($ordered[$middle-1]+$ordered[$middle])/2}}
$result=[ordered]@{events=$rows.Count;malformedLines=$bad;firstBodySamples=$ordered.Count;medianFirstBodySeconds=$median;topByCount=@($rank | Sort-Object -Property @{Expression='count';Descending=$true},id | Select-Object -First 10);topByMinutes=@($rank | Sort-Object -Property @{Expression='minutes';Descending=$true},id | Select-Object -First 10);stuckInitials=@($people.Keys | ForEach-Object {[pscustomobject]@{initials=$_;count=$people[$_]}} | Sort-Object count -Descending);buttons=@($buttons.Keys | Sort-Object | ForEach-Object {[pscustomobject]@{button=$_;count=$buttons[$_]}})}
function Encode-Html($text){[Net.WebUtility]::HtmlEncode([string]$text)}
function Table($items,$columns){'<table><tr>'+ (($columns | ForEach-Object {'<th>'+ (Encode-Html $_)+'</th>'}) -join '')+'</tr>'+ (($items | ForEach-Object {$item=$_;'<tr>'+ (($columns | ForEach-Object {'<td>'+(Encode-Html $item.$_)+'</td>'}) -join '')+'</tr>'}) -join '')+'</table>'}
$bodyText=if($null -eq $median){'Not recorded in these logs.'}else{([Math]::Round($median,1).ToString([Globalization.CultureInfo]::InvariantCulture))+' seconds ('+$ordered.Count+' parts).'}
$html='<!doctype html><html lang="en"><meta charset="utf-8"><title>IDEA Unstuck teacher summary</title><style>@page{size:letter landscape;margin:.4in}body{font:12px Segoe UI,Arial;color:#17243a;margin:20px}h1{font-size:22px}h2{font-size:15px;margin:12px 0 5px}.grid{display:grid;grid-template-columns:1fr 1fr 1fr;gap:22px}table{width:100%;border-collapse:collapse}td,th{text-align:left;padding:3px;border-bottom:1px solid #ddd}p{line-height:1.35}footer{font-size:10px;margin-top:15px}</style><h1>IDEA Unstuck - Teacher summary</h1><p>'+ $rows.Count +' unique events. '+$bad+' unreadable lines skipped. Median time to first body: '+(Encode-Html $bodyText)+'</p><div class="grid"><section><h2>Top 10 by count</h2>'+(Table $result.topByCount @('id','count','minutes'))+'</section><section><h2>Top 10 by time</h2>'+(Table $result.topByMinutes @('id','count','minutes'))+'</section><section><h2>Stuck help - initials only</h2>'+(Table ($result.stuckInitials | Select-Object -First 10) @('initials','count'))+'<h2>Buttons used</h2>'+(Table $result.buttons @('button','count'))+'</section></div><footer>Minutes include completed diagnosis intervals only. Repeated exports are deduplicated. Initials may group different students. First-body time starts at the first observed empty part; old logs without that event show "not recorded." No document or feature names are printed.</footer></html>'
[IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath),$html,[Text.UTF8Encoding]::new($false))
$jsonPath=[IO.Path]::ChangeExtension([IO.Path]::GetFullPath($OutputPath),'.json')
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8
Write-Output ('Report: '+[IO.Path]::GetFullPath($OutputPath))
Write-Output ('Events='+$rows.Count+'; malformed='+$bad+'; first-body samples='+$ordered.Count)
