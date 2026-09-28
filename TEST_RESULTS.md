# Verification report

Date: September 27, 2026 (local time). This is a **pilot build, not a fully accepted lab installation**.

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

Source and verification evidence are committed locally. Nothing has been pushed to a remote repository.
