# IDEA Unstuck handoff

Private repository: https://github.com/pina-hash/idea-unstuck

Pilot release: https://github.com/pina-hash/idea-unstuck/releases/tag/v0.1.0-pilot

## Done

The x64 .NET Framework 4.8 SOLIDWORKS Task Pane add-in, diagnosis rules, guarded rescue actions, four guided paths, local telemetry, installer/uninstaller, and SelfTest runner are implemented. The installed copy on the development PC is at `C:\Program Files\IDEA\Unstuck`.

The search update embeds the supplied 49-entry `IdeaUnstuck.Core/kb.json` unchanged. It searches locally with exact/prefix/single-typo/whole-phrasing scores, shows the top five titles, and opens inline fix, steps, why, and related links. Diagnosis More help links map to the requested articles. The 1.5-second poll is unchanged; search adds no network calls or modal dialogs.

Current search-build evidence:

- Build: **0 warnings, 0 errors**.
- Logic SelfTest: **17 PASS, 0 FAIL**, including six help cases.
- Installed-host SelfTest: **21 PASS, 0 FAIL** (the same 17 logic cases plus four host checks; do not add these as independent cases).
- Knowledge base: **49 entries, 79 related links, 12 diagnosis mappings** verified. Both requested search queries return the expected first title.
- Updated install succeeded; installed Core and AddIn hashes match the release binaries. Installed host reported zero exceptions.

Previous build evidence, retained separately: full SelfTest **26 PASS, 0 FAIL** (11 logic + 15 live SOLIDWORKS cases), two normal-startup runs **14 PASS, 0 FAIL each**, and installer install/install/uninstall/uninstall/install cycle **5 PASS, 0 FAIL**. Those results predate search and are not a substitute for the current full regression rerun. See TEST_RESULTS.md and verification/ for exact scope and logs.

The source, documentation, verification folder, and four release binaries are committed and pushed. `bin`, `obj`, and generated test parts are excluded. Release `v0.1.0-pilot` was built from code commit `9a9f1cf` and published with TEST_RESULTS.md as its notes. This handoff is a subsequent documentation-only commit on main.

## Unfinished and checks still owed

1. Rerun the full current SelfTest (expected **32 cases** if all run: 17 logic + 15 live; this is an expected count, not a passing result).
2. Finish visible search UI verification: both example queries, bold fix, numbered steps, Why, related links, diagnosis More help, and preserving query/article while polling and document state changes. The automation-created host reported a healthy pane, but its visible task pane showed Import Model. A normal desktop launch was prepared for inspection, then the user stopped Computer Use with Escape and requested immediate cloud publication. No visual pass is claimed.
3. **SOLIDWORKS 2025 on a school PC:** run installation, all SelfTests, pane checks, and startup checks. Only SOLIDWORKS 2026 SP4.1 / API 34.4.1 was available here.
4. **Second Windows login:** verify the logon initializer and automatic add-in startup under another student account; check school PowerShell/logon policy.
5. **30 minutes of normal modeling:** all lessons, dimension/extrude/cut/fillet/save, switches, native Ctrl+Z, opening existing documents, and no noticeable lag or logged exceptions.
6. **Ctrl-click export:** Ctrl-click the pane title, click Teacher export, and inspect the Desktop ZIP. The export function was tested through the installed automation interface; the reveal gesture itself was not.
7. Complete the remaining TEST_PLAN.md checks, including higher DPI, nested outlines, all guided student workflows, and deployment signing review with school IT.

No automatic endpoint highlighting or geometry repair is implemented. Unknown native errors retain their code and ask a TA. Bounding-box-based change-size evidence cannot detect every internal edit. This remains a pilot, not an accepted classroom rollout.

## Resume on a new Windows computer

Use an account with access to the private GitHub repository and a licensed working SOLIDWORKS installation with standard English part/assembly templates. Install Git, a .NET SDK (development used **10.0.401**), and the **.NET Framework 4.8 Developer Pack**. An existing 4.8 runtime alone does not supply the targeting assemblies needed to build.

```powershell
git clone https://github.com/pina-hash/idea-unstuck.git
cd idea-unstuck
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
.\release\IdeaUnstuck.SelfTest.exe --unit
.\release\IdeaUnstuck.SelfTest.exe
```

`build.ps1` executes the following build and copies the four required outputs to release:

```powershell
dotnet build IdeaUnstuck.sln -c Release
```

The default interop location is `C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist`. For a different installation, use:

```powershell
dotnet build IdeaUnstuck.sln -c Release -p:SolidWorksInteropPath="C:\your\SOLIDWORKS\api\redist"
```

Then copy `IdeaUnstuck.*.dll` from the AddIn build folder and `IdeaUnstuck.SelfTest.exe*` from the SelfTest build folder into release, as build.ps1 does. Interop types are embedded; do not ship the SOLIDWORKS interop DLLs.

Save your work and close SOLIDWORKS before installation. Run `install.bat` as administrator, approve Windows elevation, and wait for SUCCESS. Run live tests when no student modeling is in progress; the runner creates and closes its own disposable documents.

Exact installed-host commands:

```powershell
.\release\IdeaUnstuck.SelfTest.exe --addin
```

For normal-startup verification, launch SOLIDWORKS normally, wait for startup to finish, then run:

```powershell
.\release\IdeaUnstuck.SelfTest.exe --addin --startup
.\release\IdeaUnstuck.SelfTest.exe --diagnose --show-pane
.\release\IdeaUnstuck.SelfTest.exe --diagnose --export
```

`--inspect` holds the SelfTest fixture for up to five minutes, or until `release\test-results\inspect.stop` exists; remove that marker before a new inspection. Log exports go to the current user's Desktop. Native-host telemetry should be checked through that ZIP because a packaged terminal can see a virtualized AppData location.

Record actual counts and remaining failures in TEST_RESULTS.md, add verification evidence, rebuild release files if code changed, then commit and push main. Keep unperformed acceptance checks marked NOT RUN.
