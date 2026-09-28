# IDEA Unstuck

IDEA Unstuck puts short instructions and rescue buttons in the SOLIDWORKS Task Pane. It is for students opening SOLIDWORKS for the first time. It reads the active part every 1.5 seconds. A student must press a button before the helper changes units, the view, the sketch being edited, or the rollback position.

This is a classroom pilot. See [TEST_RESULTS.md](TEST_RESULTS.md) for the checks actually performed and the remaining checks before a lab rollout. SOLIDWORKS 2025 and 2026 are the targets. Only 2026 is available on the development PC.

## Install on a lab PC

1. Use a Windows PC with a licensed, working SOLIDWORKS 2025 or 2026 installation and .NET Framework 4.8. Use English part templates with Front Plane, Top Plane, and Right Plane.
2. Extract the entire release ZIP. Keep `install.bat`, `uninstall.bat`, `scripts`, and `release` together. Do not run the batch file from inside the ZIP.
3. Save students' work and close SOLIDWORKS.
4. Right-click `install.bat` and choose **Run as administrator**. Approve the Windows prompt. Wait for the line beginning **SUCCESS**. A **FAILURE** line means installation did not finish.
5. Sign out of Windows and sign back in as the student. This runs the per-user startup initializer.
6. Open SOLIDWORKS. Choose **Tools > Add-Ins**. Find **IDEA Unstuck**. Select both its **Active Add-ins** and **Start Up** checkboxes.
7. Find **IDEA Unstuck** among the Task Pane tabs on the right. If the Task Pane is hidden, enable it under **View > User Interface > Task Pane**. Select the IDEA Unstuck tab.
8. Open a part. Read the top card and do its next click. The checklist changes when SOLIDWORKS reports completed work; clicking a checklist heading only expands or hides it.
9. Before deploying to a whole class, sign into a second Windows account and run the manual checks in [TEST_PLAN.md](TEST_PLAN.md).

No Visual Studio, SDK, or targeting pack is needed on student PCs. Those tools are for building the add-in. Copying the DLL by itself does not install it.

## What the buttons do

| Button | Result |
| --- | --- |
| Find my part | Changes to an isometric view and zooms to fit. Exit a sketch first. |
| Set units to inches | Sets IPS and three decimal places. It does not scale existing geometry. One inch is 25.4 mm. |
| Get back into my sketch | Opens the selected sketch, or the last available sketch in the feature tree. |
| Exit sketch | Finishes the active sketch. It does not draw or dimension anything. |
| Roll to end | Moves the rollback bar to the end. Exit a sketch first. |
| Undo | Sends one SOLIDWORKS Undo command. |
| Start a sketch on the Front Plane | Starts a sketch only in a part with no existing sketch or body. |
| Copy for AI | Copies the state and instructions to the clipboard. The student chooses where to paste it. Nothing is sent automatically. |

After Copy for AI, the pane explains how to capture the whole window with Windows+Shift+S. The clipboard text can contain the file name and feature names. The student decides whether to share it.

Closed outlines inside other outlines can make holes. Separate outlines can also extrude successfully and make separate solid bodies. The pane explains this rather than incorrectly saying that all multiple outlines prevent extrusion. The simple first-part checklist asks for one outline.

The helper does not highlight open ends automatically, since changing the selection would violate the rule against automatic changes. It does not delete geometry, repair shapes automatically, draw, dimension, save, or choose a fillet radius for a student.

## Collect logs

1. Hold **Ctrl** and click the **IDEA Unstuck** title inside the pane.
2. Click **Teacher export**.
3. Find the new `IDEA-Unstuck-...zip` on that Windows user's Desktop. It contains today's log for that user.

Logs are local at `%LOCALAPPDATA%\IDEA\Unstuck\log-YYYYMMDD.jsonl`. Nothing uploads them. Each Windows login has its own folder. Collecting another student's log requires that student's login or normal school IT access. Logs include the Windows user name at session start/end, file names, and error messages. Keep exports under the school's normal rules for student records.

## Uninstall

1. Save your work and close SOLIDWORKS.
2. Right-click `uninstall.bat` and choose **Run as administrator**.
3. Wait for **SUCCESS**. The program directory, COM registration, startup logon entry, and the add-in's startup registry entries in local user profiles are removed.
4. Open SOLIDWORKS and check that IDEA Unstuck is gone from **Tools > Add-Ins**.

Running install or uninstall twice is supported. Your part files and teaching logs are deliberately retained. The separately installed .NET Developer Pack is a build tool and is not removed by the add-in uninstaller.

## Startup for multiple Windows users

SOLIDWORKS stores **Start Up** in each user's `HKCU\Software\SolidWorks\AddInsStartup\{962CE2F1-BF2C-46F3-A8A5-DC6081767E13}`. Registering under one administrator's account does not initialize everyone else's account.

The installer also creates the machine-wide Windows logon entry `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\IDEA Unstuck User Startup`. It runs the installed `Initialize-User.ps1` as the newly signed-in user, without an administrator prompt. The script creates that user's SOLIDWORKS startup value as DWORD 1 if it is absent. It respects an existing student's unchecked startup setting. RegAsm creates the current installing user's value immediately.

This is a logon mechanism, not a service. School policy can disable logon programs or PowerShell scripts. Sign out/in and verify with a second student login. Until that real login test has passed, multi-user automatic startup remains unverified. Users already signed in when installation occurs should sign out/in or set both checkboxes manually. Uninstall visits loaded user hives and temporarily loads unloaded local profile hives to remove this add-in's setting only.

## For the person building it

The repository contains `IdeaUnstuck.Core`, `IdeaUnstuck.AddIn`, and `IdeaUnstuck.SelfTest`. It builds an x64 .NET Framework 4.8 add-in with fixed GUID `{962CE2F1-BF2C-46F3-A8A5-DC6081767E13}`. Install the .NET SDK and the **.NET Framework 4.8 Developer Pack**. This PC uses .NET SDK 10.0.401. The Developer Pack was installed and its targeting assemblies verified before source code was written.

Build from the repository directory:

```powershell
dotnet build IdeaUnstuck.sln -c Release
```

Or run `build.ps1` to build and populate the `release` folder. The interop references come from `C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist`. If needed, supply `-p:SolidWorksInteropPath="your path"`. All three references have `EmbedInteropTypes=true` and `Private=false`. The built add-in's assembly references are checked for an unwanted dependency on the 2026 interop DLLs.

Run tests on a licensed PC with standard templates:

```powershell
.\IdeaUnstuck.SelfTest\bin\Release\net48\IdeaUnstuck.SelfTest.exe
.\IdeaUnstuck.SelfTest\bin\Release\net48\IdeaUnstuck.SelfTest.exe --unit
.\IdeaUnstuck.SelfTest\bin\Release\net48\IdeaUnstuck.SelfTest.exe --addin
.\IdeaUnstuck.SelfTest\bin\Release\net48\IdeaUnstuck.SelfTest.exe --addin --startup
```

`--addin` checks the installed connection, live pane polling, and teacher ZIP export. For `--addin --startup`, first launch SOLIDWORKS normally and wait until startup is complete. This mode attaches to the running instance and checks it without calling LoadAddIn. `--diagnose` prints the installed add-in health; adding `--export` creates a teacher ZIP through the same export function used by the button.

The release ZIP also includes `release\IdeaUnstuck.SelfTest.exe`. The live runner creates its own test parts through `SldWorks.Application`. It closes only the documents it created. It saves a test part and its last snapshot in `test-results` beside the executable. Use `--filter=fillet` to run one named case. `--inspect` holds the test instance for up to five minutes for inspection. Do not use the test runner during class modeling. A case fails if an API exception is caught, an expected state is missing, or a rescue does not produce its expected state. No unperformed check is counted as a pass.

## Behavior and limits

All SOLIDWORKS calls use a catch-and-log guard. Event handlers and button handlers are guarded too. No helper code calls MessageBox or opens a modal dialog. Installer failures are printed to the console. The live test runner disables SOLIDWORKS feature error dialogs in its own test documents.

The pane is a registered WinForms UserControl (`IdeaUnstuck.Pane`, CLSID `{73B7229D-3403-4476-B3D0-67B7A5781BA2}`) hosted with `ITaskpaneView.AddControl`. RegAsm installs and removes both COM classes. The add-in provides an explicit IDispatch interface for host callbacks and health checks.

COM work and the WinForms timer stay on SOLIDWORKS' UI thread. The timer runs at 1500 ms only for an active part. A busy flag prevents overlapping reads. Assemblies, drawings, and no-document state refresh on application events. It does not launch background threads that access SOLIDWORKS. Exceptions cannot protect against a native SOLIDWORKS crash or an API call that never returns. The requested 30-minute normal-modeling check is still required; an absolute no-freeze guarantee would be misleading.

Units are grouped with the available undo-recording API. View changes, sketch-edit mode, selection, and rollback do not have reliably supported undo grouping, so the buttons do not promise that Ctrl+Z reverses those UI operations. Undo itself sends exactly one Undo. Re-entering a sketch can temporarily roll later features back; that normal editing state is not presented as a rollback trap.

The state reader walks top-level and absorbed features, deduplicates them, and limits traversal to 1500 features and 16 levels. It records suppressed features and feature errors/warnings. It reads native contour and constraint status, not a geometric repair solver. A face consumed by later features can be unavailable until the sketch is edited. Unsupported or unknown codes keep their raw name/value and say Ask a TA. If an API read throws, diagnoses defer to a visible incomplete-check message instead of asserting a false result.

Checklist ticks record observed evidence during the current active-document visit. Opening or switching parts resets that visit. The first path verifies a saved name, but cannot know a student's last name. Change a size watches actual dimension values and a later body bounding-box change; some internal size changes do not alter the bounding box and therefore cannot complete that last check automatically. The face-cut path needs to observe the face sketch before the new cut. The fillet path combines edge selection and radius entry into the committed Fillet step because uncommitted PropertyManager choices are not safe proof of completion.

## Telemetry schema for a future dashboard

Each UTF-8 JSON Lines record has `schema: 1`, UTC ISO-8601 `utc`, a random `session` ID, `eventName`, and `data`. The file name uses the local calendar date. The logon initializer's rare `startup_error` record has no session because the add-in is not running yet.

| eventName | data fields |
| --- | --- |
| session_start, session_end | user, solidworks (API revision), addin (assembly version) |
| document_created | type (1 part, 2 assembly, 3 drawing), template |
| document_opened | name |
| document_saved | saveType, name |
| diagnosis_appeared | Id, Title |
| diagnosis_cleared | id, seconds (duration) |
| button_pressed | action: find, units, edit, exit, roll, undo, front, copy, export, or path_toggle (also path) |
| rescue_result | action, success |
| guided_step_completed | id (a1-a9, b1-b4, c1-c4, d1-d2), document |
| stuck_triggered | reason (three_minutes or five_undos), errors |
| exception | operation, type, Message, StackTrace |
| selftest_failure | name, Message |
| startup_error | message |

Diagnosis durations close on clearing, document change, or normal disconnect. A crash or power loss can leave an open interval and omit session_end. Log write failures are retained in memory and debug output; a failed disk write cannot reliably log itself. Document save events are subscribed for the active part only, since the add-in does not operate on assemblies or drawings. No timers or counters are shared between Windows users.

## SOLIDWORKS API compatibility audit

The API member inventory and links to the 2025 API Help are in [API_AVAILABILITY.md](API_AVAILABILITY.md). Each verified entry records its Availability line. Entries whose official page lacks that line are explicitly marked unconfirmed. No call is intentionally based on a 2026-only member. Embedding interop types removes the reference-version dependency, but does not replace testing on a real 2025 lab installation.

The first-availability line could not be confirmed for these used members: `ISldWorks.ActiveDoc`, `RevisionNumber`, `CloseDoc`, `ExitApp`, `Visible`, `LoadAddIn`, `AllowFailedFeatureCreation`; `IFeature.GetNextFeature`, `GetFirstSubFeature`, `GetNextSubFeature`, `GetDefinition`, `Name`, `GetNextDisplayDimension`; `IDimension.SystemValue`, `FullName`; `IPartDoc.FeatureByName`; and the `FileNewNotify2` event delegate. Their reference pages were checked in the 2025 API Help, and those lacking the line are not assigned an invented release date.
