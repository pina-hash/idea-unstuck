# IDEA Unstuck v0.3 audit

Ledger 0348, written 2026-09-28 into 2026-09-29, before and during the v0.3 build. Every claim below says what checked it. Nothing here was checked inside SOLIDWORKS: this session ran in a Linux container, and CI runs on a GitHub Windows runner. Neither has SOLIDWORKS.

## 1. What the v0.2 screenshot shows wrong

Source: `design/pane-v0.2-2026-09-28.png`, SOLIDWORKS 2026, 2026-09-28.

1. **It is not the v0.2 build.** The practice step reads "Select Front Plane, then click Sketch." `docs/wording.md` line 261 records replacing that sentence with "Click Front Plane. Click Sketch." in the Phase 4 wording pass, which shipped as v0.2.0-pilot. "My first 3D part" is the open path, and v0.2 opens "First 10 minutes". So that laptop was running an older DLL, most likely v0.1.0-pilot. v0.2's `install.bat` had to be run as administrator and ended on one line of text, so a failed or skipped upgrade was easy to miss. The v0.3 installer states the version it replaced, ends on one large PASS or FAIL line, and logs every run on the drive.
2. **No icon.** The task pane tab strip shows a blank tile for IDEA Unstuck, because v0.2 called `CreateTaskpaneView2("", ...)`. The Add-Ins list had no icon either.
3. **It opens on a wall of buttons.** Eight equal "One next click" buttons (Find my part, Set units to inches, Get back into my sketch, Exit sketch, Roll to end, Undo, Start a sketch on the Front Plane, Copy for AI) are stacked with no reason attached to any of them. A student can't tell which one applies. This reads as a debug panel.
4. **Buttons hug their text.** Each button is as wide as its label (AutoSize), so the column has a ragged right edge and nothing lines up.
5. **The check boxes are font glyphs.** The practice steps start with a small square: `□` drawn in a label (`UnstuckPane.cs` v0.2, `RenderPaths`). Segoe UI has no such glyph, so Windows falls back to another font. It isn't a control, so it can't be clicked or read by a screen reader.
6. **"Do it for me" sits right under every step.** It is the most prominent thing in the practice list, so the fastest path is to press it without trying the step.
7. **Everything is one long page.** Practice paths, help, rescue buttons and diagnoses share one scroll. The shown path's steps and the collapsed "Show:" headers are the same visual weight as everything else.
8. **Search is not first.** In v0.2 the search box sits under the title and status line, above the diagnosis cards. Nothing tells a student to type in their own words.
9. **The look is a default WinForms look** (white, flat gray buttons with black borders), unlike SOLIDWORKS and unlike the SolidWorks Day post students have open beside it.
10. **Copy for AI is a primary button.** It sits at the same weight as the rescue buttons, which puts AI in front of students who dislike it. It never needs to be the first thing tried.

## 2. Registration and per-user startup (v0.2, read from `scripts/Setup.ps1`, `scripts/Initialize-User.ps1`, `IdeaUnstuck.AddIn/Addin.cs`)

- `install.bat` runs `Setup.ps1 -Mode Install`, which requires administrator rights, refuses if a SOLIDWORKS process has our DLL loaded, copies `IdeaUnstuck.AddIn.dll`, `IdeaUnstuck.Core.dll` and `Initialize-User.ps1` to `C:\Program Files\IDEA\Unstuck`, then runs 64-bit `RegAsm /codebase`.
- `Addin.Register` (the `[ComRegisterFunction]`) writes `HKLM\SOFTWARE\SolidWorks\Addins\{962CE2F1-...}` (default 1, Title, Description) and `HKCU\Software\SolidWorks\AddInsStartup\{962CE2F1-...}` = 1 for **the account running RegAsm**.
- An `HKLM\...\CurrentVersion\Run` entry runs `Initialize-User.ps1` hidden at every Windows sign-in. It creates the user's `AddInsStartup` key with value 1 **only if the key is missing**, so a student who turned the add-in off in Tools > Add-Ins stays off.
- Uninstall reverses all of it and removes the startup key from every profile, loading unloaded hives with `reg load`.

**Verdict: the mechanism is right. It has one gap, now fixed.** Each Windows sign-in, including a student's first ever, gets the startup key from the Run entry before they can open SOLIDWORKS in the normal case. HKLM Addins is read by every installed SOLIDWORKS version, so no per-version work is needed. The gap: when a student is signed in and an administrator approves the install through UAC with different credentials, `HKCU` inside the elevated process is the administrator's hive, not the student's. So the signed-in student had no startup key until they signed out and back in, and that step was in the v0.2 README for this reason. v0.3 keeps Run plus Initialize-User and adds one step: after registering, it writes the key (only where missing) into every currently loaded user hive under `HKEY_USERS\S-1-5-21-*`. A signed-in student now gets the add-in the next time they start SOLIDWORKS, with no sign-out.

A second, smaller finding: RegAsm writes a version-specific `InprocServer32\<version>` subkey, and v0.2's upgrade path simply registered over the old one. v0.3 unregisters the installed DLL first (whatever version it is), then registers the new one.

Not changed, and why: Active Setup (`HKLM\SOFTWARE\Microsoft\Active Setup\Installed Components`) would run once per user before the desktop appears, which is slightly stronger than Run. It is a different, untested mechanism. The prompt said to keep the tested one unless it is proven wrong, and it isn't wrong.

## 3. Where the build gets the SOLIDWORKS interop assemblies

Claims from the prompt, each checked:

| Claim | Result | Checked by |
| --- | --- | --- |
| The build reads the DLLs from `C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist` (Directory.Build.props) | True. It was the only source. | Reading `Directory.Build.props` at `98a3f03` |
| That path does not exist in this container or on a GitHub runner, and the DLLs are not in the repo | True for the container (Linux, no such path) and the repo (`git ls-files` has no SolidWorks.Interop file). The runner has no SOLIDWORKS; the CI build log line "SOLIDWORKS interop from ...nuget\packages..." shows the fallback was taken. | `ls`, `git ls-files`, CI build step |
| nuget.org carries SolidWorks.Interop.sldworks/.swconst/.swpublished 32.1.0 by "avidesk", described as 2024 SP1 | True. Nuspec: authors `avidesk`, owners `Dassault Systèmes`, description "SolidWorks API (sldworks) Version 2024 SP1". Assemblies are version 32.1.0.123, public key tokens 7c4797c3e4eeac03 (sldworks), 19f43e188e4269d8 (swconst), 89a97bdc5284e6d8 (swpublished), under `lib/netstandard2.0`. | nuget.org registration API; `tools/InteropCheck interop` |
| Visiativ.SOLIDWORKS.Interop 34.0.0 exists, described as 2026 SP0 | True. Description "SolidWorks API 2026 SP 0.0", published 2026-03-10, authors "Eray Oeztuerk". Not used. | nuget.org registration API |
| Building against the older API with EmbedInteropTypes keeps one DLL working on 2025 and 2026 | Supported, with the evidence below. Not proven inside SOLIDWORKS 2025 or 2026. | See below |

**What v0.3 does.** `Directory.Build.props` still uses `SolidWorksInteropPath` when `SolidWorks.Interop.sldworks.dll` exists there, so a build on a PC with SOLIDWORKS is exactly what v0.2 did. Only when that file is missing does it add `PackageReference`s to the three 32.1.0 packages with `ExcludeAssets="all"` and `PrivateAssets="all"`. The build references them, embeds the interface definitions it uses, and copies nothing. The HintPaths are set in `Directory.Build.targets`, because the `Pkg*` path properties NuGet defines don't exist yet when `Directory.Build.props` is read. A `ReportInteropSource` target prints which DLL each project compiled against and fails if it's missing. `tools/package-usb.ps1` refuses to build a ZIP that contains any `SolidWorks.Interop*` file. No SOLIDWORKS DLL is in the repo.

**The proof that the embedded interfaces match v0.2.** `verification/` did not record interface IIDs for v0.2 (it recorded only "InteropReferences: 0" and hashes). But the v0.2 release DLLs were committed at tag `v0.2.0-pilot` (`release/IdeaUnstuck.*.dll`, built against SOLIDWORKS 2026, API 34.4.1 per TEST_RESULTS.md), and an embedded interop type carries its own GUID. `tools/InteropCheck` (new, .NET 8, reads metadata only) compares the two builds type by type. It also compares **vtable slots**: an embedded interface keeps only the methods this code calls, with `_VtblGapN_M` placeholders for the rest, so each named method's slot number can be recomputed and compared. If a method's slot differs between builds, the DLL would call the wrong function. Result, `verification/v0.3-interop-check.txt`:

- AddIn: 18 embedded types in v0.2, 20 in v0.3; all 18 shared have identical GUIDs; 11 shared interfaces, 0 slot conflicts. The 2 new ones are `IFrame` and `IModelView`, used to hand keyboard focus back to SOLIDWORKS on Esc. Their GUIDs (83A33D48-27C5-11CE-..., 83A33D4C-27C5-11CE-...) come from the 2024 package and belong to the same long-standing SOLIDWORKS interface family.
- Core: 26 and 26, all identical GUIDs; 23 shared interfaces, 0 slot conflicts.
- Every GUID also matches the 32.1.0 interop assembly.

CI repeats this comparison on every push against the DLLs at tag v0.2.0-pilot (step "Embedded SOLIDWORKS interface GUIDs and slots match v0.2") and fails on any difference.

**What this does not prove.** It shows that v0.3 asks SOLIDWORKS for the same interfaces, at the same slots, as the v0.2 build that ran on 2026. The methods new in v0.3 (`CreateTaskpaneView3`, `ISldWorks.Frame`, `IModelDoc2.ActiveView`, `IModelView.GetViewHWndx64`, `IFrame.GetHWndx64`, `ISldWorks.NewDocument` in Core) take their slots from the 2024 API. SOLIDWORKS adds methods to the end of an interface, so an older slot should be the same in 2025 and 2026. That is how COM interfaces are meant to evolve, but nobody has run these calls on either version yet. Each one is wrapped in `ApiGuard`, and the pane falls back without it: no icon, or no focus handoff.

**Package provenance, for Mr. Pina to decide.** The 32.1.0 packages are third-party uploads by the nuget.org account "avidesk" of Dassault's redistributable interop DLLs. The assembly names, versions and strong-name tokens look like Dassault's. I did not compare the files byte-for-byte with a SOLIDWORKS 2024 install, because there isn't one here. They are used only at build time and nothing from them ships. If you'd rather not depend on them, build on a PC with SOLIDWORKS (the build uses the installed DLLs automatically), or commit nothing and set `-p:SolidWorksInteropPath=...` on a machine that has them.

## 4. The help library

- `design/kb-v2.json` (99 entries at the start of the night: 40 wrong, 47 how, 12 what. Mr. Pina added answers twice during the night, and it ended at 105: 41 wrong, 52 how, 12 what. 121 synonyms, 10 keys) replaces the 49-entry `IdeaUnstuck.Core/kb.json`, which is deleted. It is embedded straight from `design/`, so the post and the add-in keep one copy. The post's data block and `design/kb-v2.json` were compared entry for entry and are identical (`JSON.stringify` equal, 2026-09-29).
- Every live diagnosis that links to an entry (26 mappings in `KnowledgeBase.DiagnosisLinks`) still resolves. All 26 target ids exist in kb-v2, so **no id needed remapping**. The loader throws at startup if any mapping, related link or common problem is missing, and SelfTest --unit checks all of them.
- **The v0.2 wording pass does not carry over.** `docs/wording.md` rewrote the 49 v0.2 articles. kb-v2 is the post's own text, which is sometimes the pre-review wording (for example the open-contour fix). I kept kb-v2 as given, because the design README says the add-in and the post must answer the same way. If the reviewed wording is better, it belongs in the post's library first. kb-v2 was scanned: no em or en dashes. "grey" and "colour" appear only inside search phrasings, where a student might type them.
- The glossary for hover help (58 SOLIDWORKS words) is copied from the post by `tools/extract-gloss.js` into `IdeaUnstuck.Core/gloss.json`. One line was changed in code: the post's "Front Plane" definition mentions a wrench picture on that page, so the add-in shows only the first sentence.

## 5. Search

`IdeaUnstuck.Core/SearchEngine.cs` ports the post's search line by line: `norm`, stopwords, `stem`, the restricted Damerau-Levenshtein `dl` (including its early "2" when lengths differ by more than one), exact 3 / prefix 2 / one typo 1.5 / two typos on long words 1, synonyms at 0.85, +4 per whole phrase (a phrase listed twice counts twice, as in the post), +1.5 for "wrong" entries on a problem word, +1.5 for "what" entries on what/mean, threshold 2, top 5 (the post shows 7).

- `tools/search-oracle/oracle.js` runs the post's own JavaScript, cut straight out of the HTML, over kb-v2. It wrote the top five ids and scores for 828 queries (the 22 pins, every title, every student phrasing, and some typo, contraction and nonsense queries) to `expected.json`.
- `tools/SearchCheck` (.NET 8, Linux or Windows) and SelfTest --unit (.NET Framework 4.8, Windows) both compare the C# port with that file. **0 differences out of 828 (after the last library update)** in this container. CI runs both, and fails if `expected.json` is stale.
- All 22 pinned queries put the stated title first in both the JavaScript and the C# port. "zzqx blorp" returns nothing and the pane shows a sentence saying so, with a way back to the common problems.
