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

Final result: **26 PASS, 0 FAIL**. This comprises **11 deterministic logic cases and 15 live SOLIDWORKS cases**. The count does not include installer tests, second-user startup, SOLIDWORKS 2025, or task-pane hosting. Full output: [verification/selftest-2026.txt](verification/selftest-2026.txt).

The live cases exercise millimeter detection and IPS/3-decimal conversion; Front Plane sketch creation; exit and re-entry; open and multiple outlines; crossing-line validation; conflicting constraints and Undo; failed fillet and Undo; rollback and recovery; sketch without body; save-state changes; assembly refusal; two solid bodies; zoom recovery; read-only state checks; and the Copy for AI prompt.

The fillet fixture needed correction during development. Direct creation or an oversized-radius edit did not reliably leave a failed feature on this service pack. The final fixture creates a valid 1 mm fillet around the block, then reduces the block's extrusion depth to 0.1 mm. This makes the existing radius too large for the thin block. SOLIDWORKS reports feature error **1**, whose enum name is `swFeatureErrorUnknown`. The helper correctly preserves that name and says Ask a TA. The test asserts a feature diagnosis and verifies that Undo clears it. Earlier failed fixture runs are not counted as passing runs; the final full run has no failures.

Native sketch validation was also checked on crossing lines. It returned an error, the rule produced a diagnosis, and selection count remained unchanged.

## Pane layout

The actual `UnstuckPane` control was opened in a separate WinForms preview at a 340-pixel outer window width. The status line, diagnosis cards, rescue labels, and checklists wrapped visibly without horizontal scrolling. Rescue buttons exceeded 36 pixels in height. This was a **standalone layout check**, not proof of in-process SOLIDWORKS Task Pane hosting. Higher-DPI scaling and normal host width still need the manual acceptance check.

## Installation and removal

`install.bat`, `uninstall.bat`, the setup script, and the logon initializer are written. PowerShell syntax checks passed. Running both batch files without elevation printed a clear FAILURE line and returned exit code 1 as intended.

The attempt to run the real installer as administrator did **not execute**: Windows' elevation request was canceled or expired, returning “The operation was canceled by the user.” This session is not elevated. No SUCCESS result is claimed. The add-in has not been installed in Program Files or registered by these scripts here.

A separate attempt to test the current-user startup registry initialization and clean up that test key was rejected before execution by automatic approval review. The only reason returned was **blocked by policy**. That registry test was not retried and is not counted as verified.

Consequently, successful installation/removal, running each twice, COM activation of the installed add-in, the real hosted pane, native Undo event delivery, and startup after restarting SOLIDWORKS are **NOT VERIFIED**.

## Remaining acceptance checks

- Approve Windows elevation and run the real install/uninstall sequence twice. Leave a final successful installation in place if desired.
- Verify task-pane hosting, actual clipboard behavior, teacher ZIP export, document save/open events, native Ctrl+Z stuck detection, and reopening SOLIDWORKS.
- Sign into a **second Windows account** and verify that the machine logon initializer sets that user's startup value and SOLIDWORKS loads the add-in. School policy may block startup scripts.
- Run the same binaries and all tests on **SOLIDWORKS 2025 / Windows 10**. That installation is not available here.
- Complete **30 minutes of normal modeling** with no noticeable lag or logged exceptions. The automated test run does not substitute for that acceptance check.
- Work through all four lessons in the real pane, including nested outlines and higher display scaling. The deterministic checklist tests verify state transitions, not the entire live student workflow.

Some requested behavior is deliberately limited: no automatic endpoint highlighting (it would alter selection without a click), no geometry deletion or automatic drawing, and no guaranteed undo grouping for view/edit-mode/rollback operations. Change-a-size body verification uses the bounding box, so an internal edit that leaves the box unchanged may need a teacher to verify it. Multiple outlines are described accurately and are not all declared invalid.

All used API members have been checked against the 2025 reference pages. Exact first-availability lines missing from older pages are listed in README.md and API_AVAILABILITY.md. Real 2025 testing remains necessary.

Source and verification evidence are committed locally. Nothing has been pushed to a remote repository.
