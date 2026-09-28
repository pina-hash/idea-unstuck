# IDEA Unstuck test plan

Use disposable parts and standard English templates. Tests change only their own fixtures. The console prints one PASS or FAIL per executed case and a final count. A caught API exception fails its case. See TEST_RESULTS.md for actual runs.

| Trap or behavior | Expected diagnosis or evidence | Rescue and assertion | Coverage |
| --- | --- | --- | --- |
| Millimeter units | millimeters | Set units to inches gives IPS, inches, 3 decimals | Live SelfTest |
| Left sketch | left-sketch | Get back into my sketch opens it | Live SelfTest |
| Open contour | open-contour | Student drags open ends together; automatic geometry repair is not provided | Detection live; repair manual |
| Crossing sketch lines | sketch-error with native status | Validation leaves the selection unchanged | Live SelfTest |
| Two side-by-side outlines | multiple-outlines | Student deletes unintended outline | Detection live; edit manual |
| Over-defined sketch | over-defined | Undo clears the conflicting relation | Live SelfTest |
| Fillet radius too big | feature error, plain radius message when code identifies it | Undo clears failed feature | Live SelfTest; fixture result reported honestly |
| Rollback bar above end | rollback | Roll to end restores one body | Live SelfTest |
| Sketch but no body | no-body | Student selects Features > Extruded Boss/Base; test extrusion creates one body | Live SelfTest |
| Never saved | unsaved | Silent test save clears diagnosis; student uses Ctrl+S | Live SelfTest |
| Assembly open | wrong-document | Part-only rescue refuses to change it | Live SelfTest |
| Two solid bodies | multiple-bodies | Student edits last Extrude; choose Merge result if shapes touch | Detection live; correction manual |
| Part zoomed away | Geometry unchanged; no invented off-screen diagnosis | Find my part restores useful view scale | Live SelfTest; framing visual |
| Front Plane sketch | In sketch, correct support name | Start sketch and Exit sketch change mode as expected | Live SelfTest |
| Copy for AI | Correct year, snapshot, causes request, screenshot sentence | Clipboard receives prompt when button is pressed | Prompt live; clipboard UI check |
| Error for 3 minutes | stuck | Fix error and stuck card clears | Deterministic clock test |
| Five undos in 1 minute | stuck | After old undos expire, stuck clears | Deterministic clock test; native event manual |
| Unknown codes | Raw value and Ask a TA | No empty text | Deterministic test |
| Automatic reads | No changed geometry, features, or edit mode | Five reads agree | Live SelfTest |
| Checklists | No completion from button presses alone | Dimensions, feature state, bodies, saved state provide evidence | Deterministic negative test and manual full lessons |

## Required classroom acceptance checks

1. **Second Windows user:** Install once as an administrator. Sign out. Sign into a different student Windows account. Wait for logon initialization, open SOLIDWORKS, and verify both add-in checkboxes and the pane. Repeat with an account that had never used SOLIDWORKS. Record whether school policy blocked the logon program.
2. **Restart:** Open the pane, close SOLIDWORKS normally, and reopen it twice. Confirm the pane loads without duplicate tabs and without an error in the log.
3. **Thirty minutes of normal modeling:** Draw, dimension, extrude, cut, fillet, save, change parts, Undo with Ctrl+Z, close documents, and reopen them. Include all four guided paths. Confirm there is no noticeable lag and no exception or startup_error in that session's JSONL. Do not substitute an idle timer test for this check.
4. **Uninstall:** Close SOLIDWORKS. Run uninstall twice. Verify the install directory, COM CLSID, SOLIDWORKS add-in entry, machine logon entry, and each tested user's startup entry are gone. Existing parts and log files should remain. Reopen SOLIDWORKS and confirm the helper is absent.
5. **SOLIDWORKS 2025:** On a Windows 10 lab PC, install the same release binaries. Run every live SelfTest case. Confirm correct task-pane hosting, keyboard Undo detection, and startup for the second Windows user. Record the exact service pack and PASS/FAIL counts.
6. **Pane width and DPI:** At normal Task Pane width, confirm there is no horizontal scroll and labels wrap. Repeat at 100%, 125%, and 150% Windows display scaling. Buttons should remain at least 36 pixels high.
7. **Nested outlines and crossing lines:** Draw an inner circle inside a rectangle, two circles side by side, a construction line, crossing lines, and a disconnected line. Check that the explanation does not claim valid holes are an error. Use SOLIDWORKS Check Sketch for Feature Usage with a TA when the contour result is ambiguous.
8. **Teacher export:** Ctrl-click the pane title, then Teacher export. Open the Desktop ZIP and check that it contains today's JSONL for the signed-in user.
9. **Units and Undo:** Check that the unit button does not rescale an existing part. Try Undo and verify its behavior for the installed SOLIDWORKS service pack. View and edit-mode changes are not promised to be undoable.

Record unperformed checks as NOT RUN, never PASS. A failed fixture is a test failure until it exercises the intended trap and verifies its recovery.
