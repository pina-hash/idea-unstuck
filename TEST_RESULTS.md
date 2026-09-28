# Verification report

## Overnight phase 4: final functional build

**49 PASS, 0 FAIL, 0 SKIP** in the complete current regression: 23 logic/managed-control cases, 15 original live cases, nine live trap cases, and two live learning cases on SOLIDWORKS 2026 SP4.1 / API 34.4.1. Build: **0 warnings, 0 errors**. Evidence: verification/overnight-phase4-final.txt and overnight-phase4-build.txt. Counts from overlapping suites must not be added as unique coverage.

The First 10 minutes path is expanded by default even with no document, has eight location hints, and completed on a real 1.000-inch circle at the origin, a 0.500-inch Mid Plane boss, and a saved file. It cannot verify a student's last name. Stuck v2 passed the two-minute boundary, four-undo threshold, cancellation matching/expiry/reset, and three real Line start/cancel events. Native cancellation is -1, not the custom PropertyManager enum. First-body telemetry excludes parts already containing a body when first observed.

The wording review covers article instructions, diagnosis guidance, lesson steps, status/rescue/error text and pane labels; docs/wording.md records 261 changed groups. Native tool names remain exact and difficult terms receive short explanations where used. This is an editorial sixth-grade target, not a validated reading-grade result. A teacher/student wording review is still owed. One final control check exposed an incomplete test fixture (BodyCount defaulted to unknown instead of zero); that fixture and a stale checklist reference in the new Undo test were corrected, and the complete rerun passed. Both paths now guard against a sketch removed by Undo; the real-host undo-to-empty-sketch check passed. The preceding 48 PASS / 1 FAIL output is retained, not counted as a pass. The new start-sketch diagnosis button now has its correct label.

Teacher-summary synthetic checks: **2 PASS, 0 FAIL**, covering aggregation, duplicate exports, malformed lines, median, initials-only output, and exclusion of file/feature names. Actual overnight inputs produced **3820 unique events, 0 malformed lines, 6 first-body samples**, median **1.734 seconds**. These are development fixtures, not student performance. The exported old-host log and current SelfTest log were combined; exact duplicate lines were removed. HTML and JSON evidence are in verification/overnight-teacher-summary.*. Browser security rejected file: URLs, so visual/one-page print-layout verification is NOT RUN. No alternate access path was used.

The **installed copy remains v0.1**. Its baseline health was connected with zero caught exceptions at the check, but that does not validate the v0.2 pane. Current installed-host visual interaction, 500-operation chaos, 45-minute soak, and lifecycle cycles remain blocked on an admin installation. No UAC prompt was launched. Search adds no network access or product modal; polling remains 1500 ms.

### Overnight run counts (including historical failures and skips)

These are separate runs, often repeating the same cases. Earlier failed outputs are retained for traceability.

| Evidence file in verification/ | Final count |
| --- | --- |
| overnight-chaos-preflight.txt | TOTAL PASS=20 FAIL=0 SKIP=1 |
| overnight-filter-repair.txt | TOTAL PASS=1 FAIL=0 |
| overnight-full-final.txt | TOTAL PASS=49 FAIL=0 SKIP=0 |
| overnight-phase1-final.txt | TOTAL PASS=34 FAIL=0 |
| overnight-phase1-unit.txt | TOTAL PASS=19 FAIL=0 |
| overnight-phase1.txt | TOTAL PASS=31 FAIL=1 |
| overnight-phase2-final.txt | TOTAL PASS=27 FAIL=1 |
| overnight-phase2-passing.txt | TOTAL PASS=28 FAIL=0 |
| overnight-phase3-edges.txt | TOTAL PASS=27 FAIL=0 SKIP=1 |
| overnight-phase4-before-undo-guard.txt | TOTAL PASS=49 FAIL=0 SKIP=0 |
| overnight-phase4-final.txt | TOTAL PASS=49 FAIL=0 SKIP=0 |
| overnight-phase4-fixture-failure.txt | TOTAL PASS=48 FAIL=1 SKIP=0 |
| overnight-phase4-learning.txt | TOTAL PASS=25 FAIL=0 SKIP=0 |
| overnight-phase4-undo-fixture-failure.txt | TOTAL PASS=48 FAIL=1 SKIP=0 |
| overnight-soak-preflight.txt | TOTAL PASS=20 FAIL=0 SKIP=1 |
| overnight-summary-test.txt | SUMMARY TEST PASS=2 FAIL=0 |
| overnight-summary-windowsps.txt | SUMMARY TEST PASS=2 FAIL=0 |

## Overnight phase 3

Edge suite: **27 PASS, 0 FAIL, 1 SKIP** (20 logic/control + 7 edge cases; school network share skipped). A 20-sketch part had 43 total features and a 2616 ms external COM read. A 300-sketch-feature part had 323 total features and a 4654 ms external COM read. These are **out-of-process full reads, not in-host poll tick timings**. Renamed non-English sketches/planes, a read-only file, drawing/no-document messages, and cut/fillet fixtures passed. The original live assembly case passed in phase 1. Evidence: verification/overnight-phase3-edges.txt.

The reader now parses detailed geometry only for the active/last sketch, while checking feature errors throughout the bounded tree. Refresh requests are coalesced and disposal is checked before rendering. A bounded 4096-sample in-host timing endpoint reports median/p95/max, poll count/concurrency, caught exceptions and blank cards; its logic test passed.

**Chaos: 0 operations executed; SKIPPED. Soak: 0 minutes executed; SKIPPED. Median/p95/max and start/end soak memory: NOT MEASURED.** Each preflight had 20 logic/control passes, 0 failures, and 1 skipped stress case. The installed v0.1 DLL lacks the current build's metrics endpoint, and replacing it requires admin approval prohibited during this unattended run. The 500-operation seeded mode and 45-minute mode are provided for the updated installation; neither is counted as passed. Their cut/fillet fixtures were exercised by the edge suite. Ten shutdown-during-tick cycles and ten add-in unload/reload cycles are likewise NOT RUN; they require validation with the current installed DLL. See Needs Mr. Pina in HANDOFF.md.

Interop runtime references remain **0 for Core and 0 for AddIn**. The API source inventory contains the original 96 entries plus 31 new guarded/API entries. A fresh 2025 page audit is PARTIAL: several official pages returned HTTP 403 or generic help shells. Unconfirmed entries are labeled in API_AVAILABILITY.md. No 2025 execution is claimed.

## Overnight phase 2

Trap suite: **28 PASS, 0 FAIL** (19 logic/control + 9 live cases) in verification/overnight-phase2-passing.txt. New live cases cover hidden tree, selection filters, first sketch on Top Plane, one-sided extrusion plus zero dimensions, radius on a full circle, five minutes without a sketch using an injected clock, rebuild needed and clearing, active Line tool, and over-defined SketchXpert availability. No geometry changes occur in diagnosis reads. docs/trap-coverage.md inventories all 25 wrong entries and explains search-only limits, including CommandManager visibility.

Test-fixture repairs: the initial radius fixture opened a Modify prompt. UI cancellation could not target its owned dialog; the blocked runner was stopped and its specific disposable Part27 was closed through the SOLIDWORKS API. The fixture now temporarily suppresses dimension input and restores the original setting in finally. A selection-filter cleanup call returned TYPE_E_ELEMENTNOTFOUND; restoring only the changed edge-filter flag fixed the test. Both repaired cases passed in the complete rerun. Initial failure records are retained separately and are not counted as passes.

## Overnight phase 1

Search completion: **34 PASS, 0 FAIL** on real SOLIDWORKS 2026 (19 logic/control tests plus 15 live cases). Managed-control tests exercised first-result navigation, bold fix, Step by step, Why, related navigation, More help, and preserving query/article across state refreshes. All diagnosis types now have help targets. Initial run found a missing assembly-card link (31 pass, 1 fail); the fixed complete rerun passed. Evidence: verification/overnight-phase1-final.txt and verification/overnight-phase1-unit.txt. Native installed-host visual checks still require installation of the changed DLL with administrator approval.

Date: September 27, 2026 (local time). This is a **pilot build, not a fully accepted lab installation**.

## Historical v0.1 search update (superseded by overnight results)

At v0.1, the supplied `kb.json` was embedded unchanged in Core (SHA-256 `57A914539723922D0D7D8BBD91DC0C9D58DE3B39E4D0CEBC481C95A1678BAD2F`). The pane now provides local ranked search, up to five title links, inline fix/steps/why/related articles, and mapped diagnosis More help links. The 1500 ms poll remains unchanged; no network call or modal was added.

- Build: **0 warnings, 0 errors**. [search-build.txt](verification/search-build.txt).
- Logic SelfTest: **17 PASS, 0 FAIL** (11 existing plus 6 help cases). Both requested queries return the expected first result. Tests cover all **49 entries, 79 related links, and 12 diagnosis mappings**, plus scoring, all four single-edit typo operations, OR matching, normalization, and result limits. [search-unit.txt](verification/search-unit.txt).
- Installed-host SelfTest: **21 PASS, 0 FAIL** (the same 17 logic cases plus 4 host cases). Installed pane connected, advanced its reads for a real part, reported zero exceptions and 49 embedded entries, and exported its log. [search-installed-host.txt](verification/search-installed-host.txt).
- Updated installation succeeded; installed Core and AddIn hashes match the tested build. [search-install.txt](verification/search-install.txt).

Commands for these runs:

```powershell
dotnet build IdeaUnstuck.sln -c Release
.\release\IdeaUnstuck.SelfTest.exe --unit
.\release\IdeaUnstuck.SelfTest.exe --addin --inspect
```

**Historical omissions at v0.1 publication (the overnight full regression now supersedes that omission):** the full 15 live-case regression rerun and visible search/article/related-link/More help interaction checks. The automation-created host reported a connected pane but the visible task pane showed Import Model; this was not counted as a visual pass. It was closed after confirming zero open documents and SOLIDWORKS was launched normally for visual inspection. The user then stopped Computer Use with Escape. At the user's direction, work moved immediately to private GitHub publication. Resume these checks before classroom acceptance. Historical successful runs below describe the previous build and do not substitute for these remaining checks.

## Previous pilot verification (before search)

## Build

Command, run from the `idea-unstuck` repository:

```powershell
dotnet build IdeaUnstuck.sln -c Release
```

Result: **SUCCESS, 0 warnings, 0 errors**. Full output: [verification/build.txt](verification/build.txt).

Environment: Windows x64, .NET SDK 10.0.401, .NET Framework 4.8 targeting pack, SOLIDWORKS Design Premium for Students 2026 SP4.1, API revision **34.4.1**. The installed interop DLL file version is **34.4.1.1011**. The missing .NET Framework 4.8 Developer Pack was downloaded from Microsoft, its Authenticode signature verified as valid Microsoft Corporation, installed with exit code 0, and its targeting assembly presence checked before coding began.

The built Core and AddIn assemblies have **zero SolidWorks.Interop runtime assembly references**. All interop references use Embed Interop Types = True. Evidence: [verification/assembly-references.json](verification/assembly-references.json).

## SelfTest on real SOLIDWORKS 2026

Command:

```powershell
.\IdeaUnstuck.SelfTest\bin\Release\net48\IdeaUnstuck.SelfTest.exe
```

Final result: **26 PASS, 0 FAIL**. This comprises **11 deterministic logic cases and 15 live SOLIDWORKS cases**. The count does not include the separate installer and hosted-add-in checks below, second-user startup, or SOLIDWORKS 2025. Full output: [verification/selftest-2026.txt](verification/selftest-2026.txt).

The live cases exercise millimeter detection and IPS/3-decimal conversion; Front Plane sketch creation; exit and re-entry; open and multiple outlines; crossing-line validation; conflicting constraints and Undo; failed fillet and Undo; rollback and recovery; sketch without body; save-state changes; assembly refusal; two solid bodies; zoom recovery; read-only state checks; and the Copy for AI prompt.

The fillet fixture needed correction during development. Direct creation or an oversized-radius edit did not reliably leave a failed feature on this service pack. The final fixture creates a valid 1 mm fillet around the block, then reduces the block's extrusion depth to 0.1 mm. This makes the existing radius too large for the thin block. SOLIDWORKS reports feature error **1**, whose enum name is `swFeatureErrorUnknown`. The helper correctly preserves that name and says Ask a TA. The test asserts a feature diagnosis and verifies that Undo clears it. Earlier failed fixture runs are not counted as passing runs; the final full run has no failures.

Native sketch validation was also checked on crossing lines. It returned an error, the rule produced a diagnosis, and selection count remained unchanged.

## Pane layout

The actual pane was verified inside SOLIDWORKS at its normal 328-pixel content width. The status, diagnosis cards, rescue buttons, and practice paths appeared, with vertical scrolling and wrapped text. Clicking **Get back into my sketch** reopened the test sketch and updated the status and diagnoses. Clicking **Copy for AI** placed the actual four-segment Front Plane sketch state and the requested causes/next-click prompt on the clipboard. The screenshot instruction appeared in the pane. Higher-DPI scaling still needs the manual acceptance check.

The initial callback object lacked IDispatch, producing an InvalidCastException during callback registration. An explicit automation interface fixes that connection failure. Handle-based hosting then produced an unusable tab on this installation. The final implementation uses the registered `IdeaUnstuck.Pane` control through `ITaskpaneView.AddControl`; its visible contents and rescue behavior were checked in the host.

Installed-host test: **14 PASS, 0 FAIL**, comprising the same 11 deterministic cases plus connection, live pane polling, and teacher ZIP export. The connection test checks the actual connected state; the polling test checks advancing render counts, active part type, and zero installed-add-in exceptions. Export calls the same function used by the Teacher export button and verifies that the Desktop ZIP exists. Evidence: [verification/addin-host-2026.txt](verification/addin-host-2026.txt). The Ctrl-click reveal gesture itself remains a manual check.

The full 26-case run was repeated with the installed pane loaded and again passed. Afterward, its health was `connected|0|82|0`: connected, no active document, 82 completed reads, zero caught add-in exceptions. Its teacher ZIP was opened and parsed. That session contained document creation and saving, diagnosis appearance/clearing, and guided-step events, with **zero exception records**. The full-day log retains two callback-registration exceptions from the earlier broken build; those are not attributed to the corrected session. Evidence: [verification/host-after-live-tests.txt](verification/host-after-live-tests.txt) and [verification/host-telemetry-summary.json](verification/host-telemetry-summary.json).

## Normal startup and reopening

The installed add-in was checked in two normally launched desktop sessions using `--addin --startup`, which attaches to the running application and never calls LoadAddIn. Both final runs returned **14 PASS, 0 FAIL**. After the first, the empty application was closed with Alt+F4; the process exited. The next completed launch loaded the add-in without a recovery prompt, and the pane again rendered the fixture and exported its log. Evidence: [verification/startup-1.txt](verification/startup-1.txt) and [verification/startup-2.txt](verification/startup-2.txt).

Earlier startup probes failed to locate the add-in in an automation-created session or before normal startup completed; one direct launch did not remain running and the next probe reported COM unavailable. Those attempts are not counted as passing startup checks. Normal startup also displayed SOLIDWORKS' recovery prompt for the earlier broken add-in. Continuing with the repaired build cleared that prompt. The second successful desktop session needed no such intervention. This verifies startup for the current Windows account, not for a second student login.

## Installation and removal

`install.bat`, `uninstall.bat`, the setup script, and the logon initializer are written. PowerShell syntax checks passed. Running both batch files without elevation printed a clear FAILURE line and returned exit code 1 as intended.

The authorized elevated sequence ran **install, install, uninstall, uninstall, install** and passed all five steps. The checks verified the install directory, COM registration, SOLIDWORKS add-in key, current-user startup value, and removal of the machine logon entry during uninstall. The final installation is present in `C:\Program Files\IDEA\Unstuck`. Evidence: [verification/installer-cycle.txt](verification/installer-cycle.txt).

RegAsm prints its standard warning about using `/codebase` with an unsigned assembly. This pilot does not include a publisher signature or strong name. Registration and activation succeeded; signing remains a deployment consideration for school IT.

## Remaining acceptance checks

- Verify the Ctrl-click teacher-button reveal, native Ctrl+Z stuck detection, and opening existing student documents in a normal classroom workflow.
- Sign into a **second Windows account** and verify that the machine logon initializer sets that user's startup value and SOLIDWORKS loads the add-in. School policy may block startup scripts.
- Run the same binaries and all tests on **SOLIDWORKS 2025 / Windows 10**. That installation is not available here.
- Complete **30 minutes of normal modeling** with no noticeable lag or logged exceptions. The automated test run does not substitute for that acceptance check.
- Work through all four lessons in the real pane, including nested outlines and higher display scaling. The deterministic checklist tests verify state transitions, not the entire live student workflow.

Some requested behavior is deliberately limited: no automatic endpoint highlighting (it would alter selection without a click), no geometry deletion or automatic drawing, and no guaranteed undo grouping for view/edit-mode/rollback operations. Change-a-size body verification uses the bounding box, so an internal edit that leaves the box unchanged may need a teacher to verify it. Multiple outlines are described accurately and are not all declared invalid.

All used API members have been checked against the 2025 reference pages. Exact first-availability lines missing from older pages are listed in README.md and API_AVAILABILITY.md. Real 2025 testing remains necessary.

Source, release binaries, and verification evidence are published in the private `pina-hash/idea-unstuck` repository. The `v0.1.0-pilot` release uses this report as its release notes. See HANDOFF.md on main for continuation instructions.
