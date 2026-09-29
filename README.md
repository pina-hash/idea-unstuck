# IDEA Unstuck

IDEA Unstuck puts short instructions and rescue buttons in the SOLIDWORKS Task Pane. It is for students opening SOLIDWORKS for the first time. It reads the active part every 1.5 seconds. A student must press a button before the helper changes units, the view, the sketch being edited, or the rollback position.

This is a classroom pilot. See [TEST_RESULTS.md](TEST_RESULTS.md) for the checks actually performed and the remaining checks before a lab rollout. SOLIDWORKS 2025 and 2026 are the targets. Version 0.3.x was built and tested with no SOLIDWORKS present (see [docs/AUDIT-v0.3.md](docs/AUDIT-v0.3.md)); v0.2 was tested on 2026.

## v0.3: what a student sees

The pane has a dark title bar with the IDEA Unstuck icon and **A A+ A++** text sizes, remembered for each Windows user. Under it are two tabs, **Help** and **Practice**.

- **Help** starts with a search box: "Type what's happening, in your own words". Up to five answers appear as the student types, each tagged **Something is wrong**, **How do I** or **What is**. Enter opens the top answer and Esc clears the box and hands the keyboard back to SOLIDWORKS. The search is the SolidWorks Day post's own search, ported line by line, over the same library (`design/kb-v2.json`).
- **Your part right now** (v0.3.1, back from v0.1 and v0.2 at Mr. Pina's request): always shown while a part is open and updated on every 1.5 s check. It gives a health line ("Looks good. Nothing to fix right now." or "1 thing to fix"), the status line (file, in or out of a sketch and on which plane, units), and the sketch report (drawing items, closed and open outlines, how defined it is, and any Check Sketch result). Then one row per current issue with what it means, the next click and **Show me how**. The first issue the helper can fix itself gets the one blue button.
- **Common problems**: six rows, then **Browse all answers**, closed until clicked and grouped by kind.
- An answer **replaces** the list: Back, the title, the fix in bold, numbered steps tagged with where to look (Left panel, Top ribbon, Bottom-right corner, Keyboard...), then **Did that fix it? Yes / Not yet**. Not yet shows related answers and "Still stuck? Raise your hand." **Read it to me** reads the answer aloud (Windows speech, offline). **Copy for AI** is a small link at the very bottom.
- **Practice**: the guided paths with real check boxes that SOLIDWORKS state ticks. One path is open at a time. **Show me where** opens a hint; **Do it for me** is a small link under a step.
- Hover any control for 0.8 s for a tip; answer steps explain the SOLIDWORKS words in them.

No network code, no account, no AI. Logs stay on the PC.

## Install on a lab PC (flash drive)

Use the release asset `IDEA-Unstuck-USB-v0.3.1.zip`. `README.txt` inside it is the one-page instruction for someone who has never done this. In short: Extract All onto a drive, then on each PC close SOLIDWORKS and double-click **Install IDEA Unstuck.cmd**. It asks for administrator rights itself and works offline. It checks .NET Framework 4.8, stops if SOLIDWORKS is open, installs to `C:\Program Files\IDEA\Unstuck` for every Windows user, lists the SOLIDWORKS versions it found, upgrades v0.2, and is safe to run twice. It ends on one large PASS or FAIL line and appends one line to `logs\<COMPUTERNAME>.txt` on the drive. **Check IDEA Unstuck.cmd** reports what is installed, and **Uninstall IDEA Unstuck.cmd** (or Settings > Apps) removes it. The five-minute acceptance check is [docs/CHECK-v0.3.md](docs/CHECK-v0.3.md).

Every Windows user gets the add-in at SOLIDWORKS startup with no box to tick. The installer registers it machine-wide, turns it on for everyone signed in right now, and a sign-in entry turns it on for each later user, including a first-ever sign-in. A student who turns it off in Tools > Add-Ins stays off.

## Rescue actions

In v0.3 these are no longer a stack of buttons. Each appears only where it fits: the one button on the Right now card, a small link under a matching answer ("Tried the steps? Find my part"), or **Do it for me** under a practice step. Every press is logged locally.

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

## Teacher summary

Put exported ZIPs or JSONL files in one folder. Run this locally:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\summarize-logs.ps1 -InputFolder C:\ClassLogs -OutputPath C:\ClassLogs\teacher-summary.html
```

The HTML report and JSON sidecar include the top ten diagnoses by count and completed minutes, median time to first body, stuck counts using initials, and button counts. Duplicate exported lines are counted once. The first-body clock starts when an empty part is first observed in the current visit; parts already containing a body are excluded. Old logs without this event show no median. Initials can combine different students, and open intervals from crashes are not counted as completed minutes. HTML targets one landscape page; print pagination still needs a visual check. It has no external assets or network calls.

## Uninstall

1. Save your work and close SOLIDWORKS.
2. Double-click **Uninstall IDEA Unstuck.cmd** on the flash drive and choose **Yes** (or use Settings > Apps > IDEA Unstuck > Uninstall).
3. Wait for the green **PASS** line. The program directory, COM registration, startup logon entry, and the add-in's startup registry entries in local user profiles are removed.
4. Open SOLIDWORKS and check that IDEA Unstuck is gone from **Tools > Add-Ins**.

Running install or uninstall twice is supported. Your part files and teaching logs are deliberately retained. The separately installed .NET Developer Pack is a build tool and is not removed by the add-in uninstaller.

## Startup for multiple Windows users

SOLIDWORKS stores **Start Up** in each user's `HKCU\Software\SolidWorks\AddInsStartup\{962CE2F1-BF2C-46F3-A8A5-DC6081767E13}`. Registering under one administrator's account does not initialize everyone else's account.

The installer also creates the machine-wide Windows logon entry `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\IDEA Unstuck User Startup`. It runs the installed `Initialize-User.ps1` as the newly signed-in user, without an administrator prompt. The script creates that user's SOLIDWORKS startup value as DWORD 1 if it is absent. It respects an existing student's unchecked startup setting. RegAsm creates the current installing user's value immediately.

This is a logon mechanism, not a service. School policy can disable logon programs or PowerShell scripts. Sign out/in and verify with a second student login. Until that real login test has passed, multi-user automatic startup remains unverified. Since v0.3 the installer also turns the add-in on for every user signed in at install time (it writes the value into each loaded user hive, only where absent), so no sign-out is needed. Uninstall visits loaded user hives and temporarily loads unloaded local profile hives to remove this add-in's setting only.

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

The SelfTest runner is built to `IdeaUnstuck.SelfTest\bin\Release\net48\IdeaUnstuck.SelfTest.exe` (it is not in the v0.3 flash-drive ZIP). The live runner creates its own test parts through `SldWorks.Application`. It closes only the documents it created. It saves a test part and its last snapshot in `test-results` beside the executable. Use `--filter=fillet` to run one named case. `--inspect` holds the test instance for up to five minutes for inspection. Do not use the test runner during class modeling. A case fails if an API exception is caught, an expected state is missing, or a rescue does not produce its expected state. No unperformed check is counted as a pass.

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
| guided_step_completed | id (a1-a9, b1-b4, c1-c4, d1-d2, e1-e8), document |
| stuck_triggered | reason (two_minutes, four_undos, or three_cancels), errors |
| first_body | seconds since first observed empty part, existingBody=false |
| exception | operation, type, Message, StackTrace |
| selftest_failure | name, Message |
| startup_error | message |

Diagnosis durations close on clearing, document change, or normal disconnect. A crash or power loss can leave an open interval and omit session_end. Log write failures are retained in memory and debug output; a failed disk write cannot reliably log itself. Document save events are subscribed for the active part only, since the add-in does not operate on assemblies or drawings. No timers or counters are shared between Windows users.

## SOLIDWORKS API compatibility audit

The API member inventory and links to the 2025 API Help are in [API_AVAILABILITY.md](API_AVAILABILITY.md). Each verified entry records its Availability line. Entries whose official page lacks that line are explicitly marked unconfirmed. No call is intentionally based on a 2026-only member. Embedding interop types removes the reference-version dependency, but does not replace testing on a real 2025 lab installation.

The original audit could not confirm first availability for these members (the overnight additions and their uncertainties are listed separately in API_AVAILABILITY.md): `ISldWorks.ActiveDoc`, `RevisionNumber`, `CloseDoc`, `ExitApp`, `Visible`, `LoadAddIn`, `AllowFailedFeatureCreation`; `IFeature.GetNextFeature`, `GetFirstSubFeature`, `GetNextSubFeature`, `GetDefinition`, `Name`, `GetNextDisplayDimension`; `IDimension.SystemValue`, `FullName`; `IPartDoc.FeatureByName`; and the `FileNewNotify2` event delegate. Their reference pages were checked in the 2025 API Help, and those lacking the line are not assigned an invented release date.

## Overnight stress modes

After installing the exact release build and closing all documents, use `IdeaUnstuck.SelfTest.exe --chaos` for 500 seeded legal operations or `--soak` for 45 minutes of modeling. Both refuse to attribute metrics from a different installed build to this release. Reports go to `test-results` beside the executable. Soak checks a 50 ms poll p95 and flags more than 256 MiB of growth after warm-up; a finite test cannot prove an unlimited memory bound. `--edges`, `--traps`, and `--learning` select their smaller live suites. A skipped prerequisite returns exit code 2; failure returns 1; a complete pass returns 0.

The development PC still has the older v0.1 add-in installed. The overnight run did not prompt for elevation. Install v0.2 before using the current host checks. See HANDOFF.md for the exact remaining acceptance work.
