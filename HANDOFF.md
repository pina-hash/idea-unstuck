# IDEA Unstuck handoff

Private repository: https://github.com/pina-hash/idea-unstuck

Pilot release: https://github.com/pina-hash/idea-unstuck/releases/tag/v0.2.0-pilot

The release rebuild contains the tested functional code from `0ab2fafecd20d4d1c4a547978d32302e438ea162`, with version 0.2.0.0 and refreshed commit metadata. Its final logic/control rerun is **23 PASS, 0 FAIL, 0 SKIP**, after the complete **49 PASS, 0 FAIL, 0 SKIP** real-host regression. File hashes and sizes are in verification/release-v0.2-manifest.json. This handoff and all verification evidence are included in the ZIP.

## Done

- Local search embeds 49 help entries with 79 valid related links. Exact/prefix/one-typo/whole-phrasing scoring, top-five titles, inline articles, related navigation, and More help for every diagnosis are implemented. Both requested queries rank correctly. The 12 original requested diagnosis mappings and all observed live diagnosis links pass.
- Nine additional live trap cases cover active tools, wrong lesson plane, one-sided first boss and missing sizes, circle radius labels, an empty part after five minutes, rebuild, SketchXpert availability, hidden tree, and selection filters. All 25 wrong entries are audited in docs/trap-coverage.md.
- First 10 minutes is expanded by default, including with no document. Its eight steps and location hints lead to an origin-centered 1.000-inch circle, 0.500-inch Mid Plane extrusion, and saving. Four earlier lesson paths remain available. Saved-state checks cannot identify a student's last name.
- Stuck v2 uses two unchanged minutes, four undos in 60 seconds, or three matched starts/cancellations of one command within two minutes. Native command events and cleanup are implemented; a native Line cancel fixture passes.
- Student wording was reviewed throughout articles and pane guidance. docs/wording.md records 261 changed groups. Actual classroom sixth-grade readability remains to be confirmed.
- Teacher summary reads offline ZIP/JSONL exports, deduplicates events, and produces HTML plus JSON with diagnosis counts/minutes, median first-body time, stuck initials, and buttons. Synthetic checks and actual overnight log aggregation passed.
- Poll timing/concurrency counters, coalesced refreshes, disposal guards, and bounded samples are implemented. Detailed sketch reads are limited to the active/last sketch; feature errors are still checked across the bounded tree. The timer remains 1500 ms. No network or new modal dialog was added to the product.

## Test results

- Full final functional SelfTest: **49 PASS, 0 FAIL, 0 SKIP** (23 logic/control + 26 real SOLIDWORKS cases). Build: **0 warnings, 0 errors**.
- Trap suite at phase 2: **28 PASS, 0 FAIL**. Edge suite at phase 3: **27 PASS, 0 FAIL, 1 SKIP** (network share). Learning suite before final wording: **25 PASS, 0 FAIL**. These overlap with the full suite.
- Teacher-summary synthetic checks: **2 PASS, 0 FAIL**. Actual overnight aggregate counts are in TEST_RESULTS.md and verification/overnight-teacher-summary.json.
- Core/AddIn have **zero SolidWorks.Interop runtime assembly references**. A 2025 API audit lists all unconfirmed uses; no 2025 execution is claimed.
- Chaos: **0 operations**, skipped. Soak: **0 minutes**, skipped. Poll median/p95/max and start/end soak memory: **not measured**. Ten shutdown-during-tick trials and ten unload/reload cycles: **not run**.
- Historical v0.1 installed-host, startup and installer successes are retained in TEST_RESULTS.md. They do not validate the new v0.2 installed pane. The development machine still has the older v0.1 DLL installed.

## Failed or skipped, and why

Earlier fixture failures (assembly help link, radius input dialog, selection-filter cleanup, and missing empty-body setup in a new control assertion) were repaired and rerun. The final complete functional suite has no failures. Original failure evidence is retained in verification/.

Current installed-host validation and stress/lifecycle work could not run against the changed DLL: this session is non-elevated and cannot replace files in Program Files. The overnight instructions prohibit waiting for UAC or user input. The stress modes detect the old installed build and skip instead of reporting its metrics as current.

No reachable school network share was provided. No second Windows login or SOLIDWORKS 2025 PC was available. Some 2025 help pages returned 403 or generic help shells, so the API audit remains partly unconfirmed. The browser rejected local file: URLs, preventing visual print-layout verification of the teacher HTML. Its data tests passed.

Search-only traps and reasons: delete-part/enter-repeat/shape-jumped/dragged-arrow depend on student intent; lost-part/view-tilted cannot distinguish intentional view changes; dim-box preferences cannot prove a specific input window should be present; crashed cannot be diagnosed after the in-process host exits or blocks; toolbar-gone has no verified whole-CommandManager visibility getter here. Blue/under-defined colors can be valid, so they do not alone trigger an error. Full details: docs/trap-coverage.md.

No automatic geometry repair or endpoint highlighting is implemented. No student document changes without a click. Large-part external COM reads were slow (about 2.6/4.7 seconds for 20/300 sketches); these are not in-host poll timings. The 50 ms p95 performance target is not yet proven.

## Needs Mr. Pina

1. Save work and close SOLIDWORKS. Install this final v0.2 build with install.bat as administrator. This is the first blocked step; no UAC prompt was launched overnight.
2. Open the new installed pane. Run --addin and normal-startup checks below. Visually verify search, related links, all diagnosis More help links, the first lesson, hints, and preserved reading during polling.
3. With the exact current DLL loaded and no documents open, run --chaos (500 operations) and --soak (45 minutes). Record the metrics and memory JSON. Investigate p95 above 50 ms or sustained memory growth. The 256 MiB warmed-run limit is only a finite leak screen.
4. Perform ten shutdown-during-tick trials and ten Tools > Add-Ins unload/reload cycles; record crashes, duplicate panes, handlers, and exceptions. This lifecycle procedure is not automated yet.
5. Test **SOLIDWORKS 2025 on a school PC**, including all SelfTests and uncertain API calls. Test **a second Windows login**, including the startup initializer and school policy.
6. Check DPI at **100%, 125%, and 150%**, pane width, wrapping, and buttons.
7. Do **30 minutes of student-style modeling** through all five paths, including native Ctrl+Z and document switches. Separately perform the **Ctrl-click export** reveal, Teacher export, and ZIP inspection.
8. Supply a reachable network part file. Check read-only/network behavior on the school setup. Print the teacher HTML to confirm one-page layout. Read the wording with students and a teacher before classroom rollout.

## Resume on another Windows computer

Use a licensed SOLIDWORKS installation with standard English templates and an account allowed into the private repo. Install Git, a .NET SDK (development used 10.0.401), and the **.NET Framework 4.8 Developer Pack**. An installed runtime alone is not the Developer Pack.

```powershell
git clone https://github.com/pina-hash/idea-unstuck.git
cd idea-unstuck
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
.\release\IdeaUnstuck.SelfTest.exe --unit
.\release\IdeaUnstuck.SelfTest.exe
```

Exact underlying build:

```powershell
dotnet build IdeaUnstuck.sln -c Release
```

For a different SOLIDWORKS installation path:

```powershell
dotnet build IdeaUnstuck.sln -c Release -p:SolidWorksInteropPath="C:\your\SOLIDWORKS\api\redist"
```

build.ps1 copies Core.dll, AddIn.dll, SelfTest.exe and its config into release. If building with a custom interop path, copy these four outputs as build.ps1 does. Never ship SOLIDWORKS interop DLLs. Save work and close SOLIDWORKS before running install.bat as administrator. Run live tests outside student modeling; they create and close their own fixtures.

After installing the exact build:

```powershell
.\release\IdeaUnstuck.SelfTest.exe --addin
# Launch SOLIDWORKS normally before the startup check:
.\release\IdeaUnstuck.SelfTest.exe --addin --startup
.\release\IdeaUnstuck.SelfTest.exe --diagnose --show-pane
.\release\IdeaUnstuck.SelfTest.exe --diagnose --export
.\release\IdeaUnstuck.SelfTest.exe --edges
.\release\IdeaUnstuck.SelfTest.exe --chaos
.\release\IdeaUnstuck.SelfTest.exe --soak
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-summary.ps1 -WorkFolder .\release\test-results\summary-check
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\summarize-logs.ps1 -InputFolder C:\ClassLogs -OutputPath C:\ClassLogs\teacher-summary.html
```

Exit codes: 0 complete pass, 1 failure, 2 skipped prerequisite. Do not count skips as passes. --inspect waits up to five minutes for UI inspection; omit it for unattended work. Teacher exports go to the Windows Desktop. A packaged terminal can see virtualized AppData, so export native-host logs through the add-in.

Record new evidence in verification/ and TEST_RESULTS.md. Rebuild release files after code changes. Commit and push each completed phase. This is a pilot; the pending checks above are required before classroom acceptance.
