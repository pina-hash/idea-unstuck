# Trap coverage and detection limits

All guidance is read-only. A reported state is not proof that a student's choice is wrong. Lesson-specific advice explicitly names the lesson.

| Knowledge-base wrong entry | Detection / limitation |
| --- | --- |
| open-contour | Native sketch contours report open outlines; original live fixture. |
| multi-contour | Multiple closed outlines / native CheckFeatureUse error; original live fixtures. Valid holes are not called invalid. |
| left-sketch | Sketch exists, no body, not editing; original live fixture. |
| delete-part | A wish to start over is student intent; no API fact can establish it. Search-only guidance. |
| units-mm | Native linear-unit setting; original live fixture. |
| red-sketch | Native constrained status. New live fixture confirms SketchXpert is enabled; its menu is suggested, never opened automatically. |
| colors | Native constraint state is included in sketch state/Copy for AI. Blue can be valid; no extra error card for being under-defined alone. |
| lost-part | Find my part rescue is tested. View scale is readable, but it cannot reliably distinguish a lost part from intentional zoom/pan. No invented off-screen diagnosis. |
| red-x | Native feature errors and warnings; original live failed-fillet fixture. |
| rollback | Native feature rollback flags; original live fixture. |
| still-drawing | GetRunningCommandInfo identifies active Line, Circle, or Rectangle. The Line fixture proves the trap; other command IDs are not guessed. |
| enter-repeat | A running command is observable, but whether the student intended to repeat it is not. Search-only. |
| wrong-select | Active selection filters are read through the native API; the live edge-filter fixture verifies the message and restores the original flags. |
| toolbar-gone | CommandManager visibility has no verified whole-manager getter here. Toolbar visibility applies to individual toolbars; CommandTabVisible is not equivalent. Search-only; manual check owed. |
| tree-gone | GetFeatureManagerWidth becomes zero when HideFeatureManager hides the tree; live fixture verifies this and restores visibility. |
| view-tilted | View orientation is readable, but intentional rotation and student difficulty are not distinguishable. Search-only. |
| extrude-direction | First active boss uses a non-Mid-Plane end condition. Advisory for this lesson; live one-sided fixture. |
| two-bodies | Native solid-body count; original live fixture. |
| radius-not-dia | One non-construction full circle and radial display-dimension type. Mixed geometry is deliberately excluded to avoid blaming an unrelated arc. Live radius fixture. |
| crashed | An in-process add-in cannot report after its host exits or while the UI thread is blocked. No claim of crash immunity. |
| shape-jumped | Geometry can move after a valid size edit. No reliable API fact proves an unintended jump. Search-only. |
| dim-box | Dimension-value entry preference exists, but it does not prove that a particular dialog should be open. No automatic dialog/setting change in the add-in. |
| wrong-plane | First sketch's support differs from the first standard reference plane. Uses plane identity/name from the model, including renamed planes, and only advises before a body exists. |
| rebuild | Native NeedsRebuild2 > 0; dimension-change fixture verifies appearance and clearing. |
| dragged-arrow | Size changes do not expose whether they were accidental or caused by a drag. Search-only. |

Additional lesson guidance: zero dimensions in a sketch supporting a body; no sketch after five minutes observing the same empty part. The latter uses native part state with an injected clock in SelfTest so the boundary is deterministic. Switching documents resets that timer.

New API first-availability evidence and remaining 2025 uncertainty belong in API_AVAILABILITY.md. Do not infer compatibility from a successful 2026 run alone.
