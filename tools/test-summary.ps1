param([Parameter(Mandatory=$true)][string]$WorkFolder)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $WorkFolder -Force | Out-Null
$lines=@(
    @{schema=1;utc='2026-09-28T01:00:00Z';session='one';eventName='session_start';data=@{user='Alex Student'}},
    @{schema=1;utc='2026-09-28T01:00:01Z';session='one';eventName='diagnosis_appeared';data=@{Id='feature:PRIVATE_NAME:1'}},
    @{schema=1;utc='2026-09-28T01:00:02Z';session='one';eventName='stuck_triggered';data=@{reason='four_undos'}},
    @{schema=1;utc='2026-09-28T01:01:01Z';session='one';eventName='diagnosis_cleared';data=@{id='feature:PRIVATE_NAME:1';seconds=60}},
    @{schema=1;utc='2026-09-28T01:01:02Z';session='one';eventName='button_pressed';data=@{action='copy'}},
    @{schema=1;utc='2026-09-28T01:01:03Z';session='one';eventName='first_body';data=@{seconds=30;existingBody=$false}},
    @{schema=1;utc='2026-09-28T01:01:04Z';session='one';eventName='first_body';data=@{seconds=60;existingBody=$false}}
) | ForEach-Object { $_ | ConvertTo-Json -Compress -Depth 5 }
$inputDir=Join-Path $WorkFolder 'input'
New-Item -ItemType Directory -Path $inputDir -Force | Out-Null
$file=Join-Path $inputDir 'test.jsonl'
$lines | Set-Content -LiteralPath $file -Encoding UTF8
Copy-Item -LiteralPath $file -Destination (Join-Path $inputDir 'duplicate.jsonl') -Force
Add-Content -LiteralPath $file -Value 'bad JSON'
$out=Join-Path $WorkFolder 'test-summary.html'
& (Join-Path $PSScriptRoot 'summarize-logs.ps1') -InputFolder $inputDir -OutputPath $out
$r=Get-Content -LiteralPath ([IO.Path]::ChangeExtension($out,'.json')) -Raw | ConvertFrom-Json
if($r.events -ne 7 -or $r.malformedLines -ne 1 -or $r.medianFirstBodySeconds -ne 45 -or $r.topByCount[0].count -ne 1 -or $r.topByCount[0].minutes -ne 1 -or $r.stuckInitials[0].initials -ne 'AS' -or $r.buttons[0].count -ne 1){throw 'Summary aggregation assertion failed.'}
$html=Get-Content -LiteralPath $out -Raw
if($html.Contains('Alex Student') -or $html.Contains('PRIVATE_NAME')){throw 'Summary exposed a full name or feature name.'}
Write-Output 'SUMMARY TEST PASS=2 FAIL=0 (aggregation/deduplication; privacy)'
