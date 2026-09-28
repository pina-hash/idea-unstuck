# Student wording review

Reviewed the 49 article titles, fixes, explanations, and all steps, plus diagnosis cards, guided paths, rescue messages, status text, error translations, and pane labels. Existing short wording was retained where clear. Student search phrasings remain unchanged. The table records edits; native tool names and unknown API error codes stay exact so a TA can identify them. New terms receive brief plain-language explanations where used. This is an editorial sixth-grade target, not a measured or classroom-certified reading score. A student/teacher read-through is still owed.

The pass also removes guarantees about recovery files, warning-free parts, and arbitrary maximum extrusion depth. Changing units no longer labels all previous values wrong.

| Location | Old wording | New wording |
| --- | --- | --- |
| open-contour.fix | Find the gap in your outline and drag one loose end onto the other until it snaps. | Find the gap. Drag one loose end onto the other end until it snaps. |
| open-contour.why | An extrude pushes your outline into 3D, like pouring plastic into a mold. If the outline has a gap, the plastic leaks out, so SolidWorks refuses. Every outline must be one closed loop. | A solid extrude makes a 3D shape from a closed outline. A gap can stop this lesson’s extrude. |
| open-contour.steps[0] | Zoom in on every corner with the scroll wheel. A gap can be too small to see from far away. | Zoom in on each corner with the scroll wheel. Small gaps can be hard to see. |
| open-contour.steps[1] | Open the Tools menu, then Sketch Tools, then Check Sketch for Feature. Click Check. It tells you where the open ends are. | Click Tools > Sketch Tools > Check Sketch for Feature. Click Check. Read the result. |
| open-contour.steps[2] | Click and drag one loose end onto the other end. Let go when a small orange square appears. That square means the two ends are joined. | Drag one loose end onto the other end. Check that the ends join. |
| open-contour.steps[3] | Delete any tiny extra line you find. Click it, press Delete. | Click an extra line you do not need. Press Delete. |
| open-contour.steps[4] | Try the extrude again. | Click Features > Extruded Boss/Base again. |
| multi-contour.fix | Erase the extra pieces so there is one outline. Circles inside it are fine: they become holes. | Check which outlines you need. Remove only the extra lines. Keep any holes you meant to draw. |
| multi-contour.why | A hook is one outline with holes inside it. Two outlines side by side, or lines that cross, give SolidWorks more than one possible shape and it cannot guess which one you mean. | An outline inside another can make a hole. Side-by-side outlines can make separate bodies. Crossing or extra lines may stop the tool. |
| multi-contour.steps[0] | Look for lines that cross each other or stick out past a corner. | Look for lines that cross or stick out past a corner. |
| multi-contour.steps[1] | On the Sketch tab, click Trim Entities. Choose Power trim. | Click Sketch > Trim Entities. Choose Power trim. |
| multi-contour.steps[2] | Hold the left mouse button and drag across the extra pieces. They disappear as you cross them. | Drag across each extra piece with the left mouse button held down. |
| multi-contour.steps[3] | Look for a line drawn twice on top of itself: drag a line away and see if another one is under it. Delete the copy. | Check for lines drawn on top of each other. Ask a TA before removing a line you are unsure about. |
| multi-contour.steps[4] | Run Tools, Sketch Tools, Check Sketch for Feature to confirm. | Click Tools > Sketch Tools > Check Sketch for Feature. Click Check. |
| left-sketch.fix | Click the sketch in the tree on the left, then click Edit Sketch (the pencil). | Click the sketch in the left tree. Click Edit Sketch. |
| left-sketch.why | Leaving a sketch does not lock it. Every sketch and feature stays editable forever. Editing is how you change a part, which is why you never need to start over. | Exit Sketch ends drawing mode. Edit Sketch lets you change the drawing later. |
| left-sketch.steps[0] | Find the list on the left side of the screen. That is the tree: every step of your part, in order. | Find the tree: the step list on the left. |
| left-sketch.steps[1] | If you already extruded, click the small arrow next to Boss-Extrude1. Your sketch (Sketch1) is inside it. | Click the arrow next to Boss-Extrude1 if you already made a 3D shape. Look for Sketch1 inside it. |
| left-sketch.steps[2] | Click Sketch1. A small bar of icons pops up next to your mouse. | Click Sketch1 in the tree. |
| left-sketch.steps[3] | Click Edit Sketch, the icon with a pencil. Your sketch lines light up and you can change them. | Click Edit Sketch, the pencil icon. |
| left-sketch.steps[4] | When you finish, click the Exit Sketch icon in the top-right corner of the part area. | Click Exit Sketch when you finish. |
| delete-part.fix | Do not delete it. Press Ctrl+Z to undo, or edit the step that went wrong. It is almost always faster. | Press Ctrl+Z once. Check what changed. You can also edit the step that went wrong. |
| delete-part.why | Starting over throws away every correct step with the one wrong one. In CAD you fix the step that broke. The tree is there so you can go back to any step. | Undo or Edit Feature can keep the steps that already work. |
| delete-part.steps[0] | Press Ctrl+Z. Each press undoes one step. Keep going until it looks right again. | Press Ctrl+Z once. Check the result before pressing it again. |
| delete-part.steps[1] | If one feature is wrong, right-click it in the tree and pick Edit Feature. | Right-click the wrong feature, a step in your part. Click Edit Feature. |
| delete-part.steps[2] | If the shape is wrong, click the sketch in the tree and pick Edit Sketch. | Right-click the sketch if its lines need changes. Click Edit Sketch. |
| delete-part.steps[3] | Still stuck after that? Ask a TA before you delete anything. | Ask a TA before deleting work you may still need. |
| units-mm.fix | You are in millimeters. Click the units at the bottom-right corner and pick IPS. | Click the units in the bottom-right corner. Choose IPS for inches. |
| units-mm.steps[0] | Look at the bottom-right corner of the SolidWorks window. It shows the unit system, like MMGS. | Find the unit label at the bottom-right. MMGS uses millimeters. |
| units-mm.steps[1] | Click it and pick IPS (inch, pound, second). | Click the unit label. Choose IPS, which uses inches. |
| units-mm.steps[2] | Your part does not change size. Only the numbers change to inches. | Check the sizes shown. Changing units does not scale your shape. |
| units-mm.steps[3] | If you typed numbers while in millimeters, they are wrong now. Retype them in inches. | Check each size against your assignment. Change only values that do not match. |
| units-mm.steps[4] | To make three decimal places show: Options (the gear at the top), Document Properties tab, Units, set Length decimals to .123. | Click Options, the gear icon. Click Document Properties > Units. Set Length decimals to .123. |
| red-sketch.fix | Press Ctrl+Z to undo the last dimension or relation you added. | Press Ctrl+Z to undo the last size or drawing rule. Check whether the red marks clear. |
| red-sketch.steps[0] | Look at the bottom of the screen. It says Over Defined when two rules are fighting. | Look for Over Defined at the bottom. It means some drawing rules conflict. |
| red-sketch.steps[1] | Press Ctrl+Z once. That usually removes the rule that caused it. | Press Ctrl+Z once. Check whether the red lines clear. |
| red-sketch.steps[2] | Or find the red or yellow dimension and delete it. | Check the red or yellow size labels. Ask a TA which rule to remove. |
| red-sketch.steps[3] | For help finding it: click the words Over Defined at the bottom. SketchXpert opens. Click Diagnose, pick a solution, click Accept. | Click Tools > Sketch Tools > SketchXpert. Click Diagnose. Review a proposed change before clicking Accept. |
| colors.fix | Blue means the line can still move. Black means dimensions lock it in place. Both are fine. | Blue lines can still move. Black lines have fixed sizes and positions. Either color can work. |
| colors.why | A fully defined sketch (all black) behaves predictably when you change a number. It is good practice, not a requirement for your first part. | Dimensions and relations are drawing rules. They hold sizes and positions in place. |
| colors.steps[0] | Drag a blue line. It moves, because nothing locks it yet. | A blue line can still move. Check its size before dragging it. |
| colors.steps[1] | Add dimensions until the lines turn black. Black lines stay put when you change other things. | Add needed sizes with Smart Dimension. Black lines have their size and position set. |
| colors.steps[2] | The bottom of the screen says Under Defined or Fully Defined. | Read Under Defined or Fully Defined at the bottom. These words describe how much can still move. |
| lost-part.fix | Put your mouse over the part area and press F. It zooms to fit your whole part. | Move the mouse over the drawing area. Press F to fit the part in the view. |
| lost-part.why | The part is almost never gone. The view moved, the rollback bar hid it, or the body was hidden. None of those delete anything. | Zoom, a hidden body, or the rollback bar can hide a part from view. Check the left tree if F does not help. |
| lost-part.steps[0] | Move the mouse over the big part area and press F. | Move the pointer over the part area. Press F to fit the view. |
| lost-part.steps[1] | Still nothing? Press the spacebar and double-click Isometric. | Press the spacebar if the view is still unclear. Double-click Isometric, a view from a corner. |
| lost-part.steps[2] | Look in the tree for a gray line with a yellow-and-black bar above your features. Right-click the bar and pick Roll to End. | Find the rollback bar in the left tree. Right-click it. Click Roll to End. |
| lost-part.steps[3] | Open the Solid Bodies folder in the tree. If your body is hidden, right-click it and pick Show. | Open Solid Bodies in the tree. Right-click a hidden body. Click Show. |
| red-x.fix | Hover over the first one from the top to read what it says, then right-click it and pick Edit Feature. | Point at the first warning in the left tree. Read its message. Right-click that step. Choose Edit Feature. |
| red-x.steps[0] | Find the FIRST feature with a mark, counting from the top of the tree. Fix that one first: it often causes the others. | Find the first marked feature from the top of the tree. Start there. |
| red-x.steps[1] | Hover the mouse over it and read the message. | Hold the pointer over its mark. Read the message. |
| red-x.steps[2] | Right-click it. Pick Edit Feature to change its settings, or Edit Sketch to change its shape. | Right-click the feature. Choose Edit Feature for settings or Edit Sketch for lines. |
| red-x.steps[3] | A yellow triangle is a warning: the part still works. A red X means that feature failed. | A yellow triangle is a warning to check. A red X means the step failed. |
| red-x.steps[4] | Common cause: a Fillet radius that is too big. Make it smaller. | Check the Fillet size if the error names a fillet. Try a smaller radius, the size of the round. |
| rollback.fix | Right-click the yellow and black bar and pick Roll to End. | Right-click the yellow-and-black bar in the left tree. Choose Roll to End. |
| rollback.why | That bar is a time machine. Everything below it is switched off, so you can see the part as it was earlier. Somebody dragged it by accident. | The rollback bar marks how far the model is built. Steps below it are paused. |
| rollback.steps[0] | Find the yellow and black bar in the tree. | Find the rollback bar in the left tree. It marks where the part stops building. |
| rollback.steps[1] | Right-click it and pick Roll to End. Or drag it to the very bottom. | Right-click the bar. Click Roll to End. |
| still-drawing.fix | Press Esc. That stops the tool. | Press Esc to stop the drawing tool. |
| still-drawing.steps[0] | Press Esc once or twice. | Press Esc. Press it again if the drawing tool stays on. |
| still-drawing.steps[1] | Delete any extra lines: click one, press Delete. | Click an extra line you do not need. Press Delete. |
| enter-repeat.fix | Press Esc to close it. Pressing Enter repeats the last tool you used. | Press Esc to stop the tool. Enter can repeat the last tool. |
| enter-repeat.steps[0] | Press Esc. | Press Esc to stop the tool. |
| enter-repeat.steps[1] | If a panel opened on the left, click the red X at its top. | Click the red X on the open settings panel if you want to cancel that tool. |
| wrong-select.fix | Right-click near it, choose Select Other, and pick the one you want from the list. | Press F6 if a selection filter is on. Otherwise, right-click near the item. Choose Select Other. |
| wrong-select.why | SolidWorks picks whatever is closest to the mouse. Thin parts put faces and edges very close together. | A selection filter limits what you can click. Select Other helps when faces and edges overlap in the view. |
| wrong-select.steps[0] | Rotate the view so the face or edge is facing you: hold the middle mouse button and drag. | Turn the view by dragging with the middle mouse button held down. |
| wrong-select.steps[1] | Or right-click near it and choose Select Other. Move through the list until the right one lights up, and click it. | Right-click near the item. Choose Select Other. Click the item you want in the list. |
| wrong-select.steps[2] | Zoom in with the scroll wheel so edges are farther apart. | Zoom in with the scroll wheel to separate nearby edges. |
| toolbar-gone.fix | Right-click on any tab at the top (like Features) and turn on the tab you are missing. | Right-click near the top tabs. Check Enable CommandManager. Turn on the missing tab. |
| toolbar-gone.steps[0] | Right-click on a tab at the top, like Features or Sketch. | Right-click a top tab, such as Features or Sketch. |
| toolbar-gone.steps[1] | In the menu, point at Tabs and check the one that is missing. | Point to Tabs. Check the missing tab. |
| toolbar-gone.steps[2] | If there are no tabs at all: right-click any empty spot at the top and check CommandManager. | Right-click an empty spot at the top if all tabs are gone. Check CommandManager, the top tool panel. |
| tree-gone.fix | Press F9. | Press F9 to show the list on the left. |
| tree-gone.why | The left panel shares its space. When a tool is open, its settings replace the tree until you finish. | F9 hides or shows the left panel. A tool’s settings can also cover the tree until you finish the tool. |
| tree-gone.steps[0] | Press F9. It shows or hides the tree. | Press F9 to show the left tree. |
| tree-gone.steps[1] | If the left side shows settings instead of the tree, a tool is still open. Click the green check to finish it or the red X to cancel. | Finish or cancel the open tool if settings cover the tree. Use the green check to finish or the red X to cancel. |
| view-tilted.fix | Press Ctrl+8 to look straight at the sketch, or press the spacebar and pick Front. | Press Ctrl+8 while editing the sketch. This looks straight at the sketch. |
| view-tilted.why | Drawing on a tilted view makes lines land at angles you did not mean. Always look straight at the sketch while you draw. | Looking straight at the sketch makes its shapes easier to see. Turning the view does not tilt the sketch plane. |
| view-tilted.steps[0] | Inside a sketch, press Ctrl+8. The view turns to face the sketch. | Press Ctrl+8 while editing a sketch. This faces the sketch straight on. |
| view-tilted.steps[1] | Or press the spacebar and double-click Front. | Press the spacebar for other views. Double-click Front. |
| extrude-direction.fix | Edit the extrude and change Blind to Mid Plane. | Right-click the Extrude in the left tree. Choose Edit Feature. Change Direction 1 to Mid Plane. |
| extrude-direction.steps[0] | Right-click Boss-Extrude1 in the tree and pick Edit Feature. | Right-click Boss-Extrude1 in the tree. Click Edit Feature. |
| extrude-direction.steps[1] | Under Direction 1, open the drop-down that says Blind and pick Mid Plane. | Open the end-setting list under Direction 1. Choose Mid Plane. |
| extrude-direction.steps[2] | Type the full thickness, for example 0.500. Half goes each way. | Type the full thickness, such as 0.500 inches. Half extends each way. |
| two-bodies.fix | Edit the last extrude and check Merge result, or change it so it touches the rest of the part. | Edit the last Extrude. Check Merge result. The new shape must touch the old shape to join it. |
| two-bodies.why | A hook must be one solid piece. Two bodies that only look connected will print as two parts that fall apart. | This lesson needs one solid body: one connected 3D shape. Two separate bodies may be correct in other projects. |
| two-bodies.steps[0] | Open the Solid Bodies folder in the tree. If it says (2), you have two separate pieces. | Open Solid Bodies in the tree. A count of 2 means two separate 3D shapes. |
| two-bodies.steps[1] | Right-click the last extrude, pick Edit Feature, and check Merge result. | Right-click the last Extrude. Click Edit Feature. Check Merge result. |
| two-bodies.steps[2] | If it still makes two, the new piece does not touch the old one. Edit its sketch so they overlap. | Check whether the new shape touches the old shape. Edit its sketch if the two should touch. |
| radius-not-dia.fix | Right-click the dimension, open Display Options, and pick Display As Diameter. | Right-click the size label. Choose Display Options > Display as Diameter. |
| radius-not-dia.why | Arcs are measured by radius and full circles by diameter. The hook rules are written as diameters, so show them that way. | Radius is half the full width. Diameter is the full width. This lesson gives the circle’s diameter. |
| radius-not-dia.steps[0] | Right-click the dimension. | Right-click the size label. |
| radius-not-dia.steps[1] | Point at Display Options and click Display As Diameter. | Point to Display Options. Click Display As Diameter, which shows full width. |
| radius-not-dia.steps[2] | Next time, click a full circle, not an arc. A circle gets a diameter automatically. | Click a full circle when adding its size with Smart Dimension. Check for the diameter symbol. |
| crashed.fix | Wait 30 seconds. If it closed, open SolidWorks again and look for Document Recovery on the right. | Wait briefly if SOLIDWORKS is busy. If it closed, reopen it. Look for Document Recovery. |
| crashed.why | Autosave keeps copies every few minutes, but only a real save is safe. | Recovery copies may be available. They depend on the backup settings. Save often with Ctrl+S. |
| crashed.steps[0] | Give it 30 seconds before clicking anything. It may be thinking. | Wait 30 seconds. SOLIDWORKS may still be working. |
| crashed.steps[1] | If it closed, reopen SolidWorks. A Document Recovery panel appears on the right side with your last autosave. | Reopen SOLIDWORKS if it closed. Look for Document Recovery; a saved recovery copy may be available. |
| crashed.steps[2] | Open it and immediately use Save As to save it with your name. | Open a recovery copy if one is listed. Use File > Save As to keep it. |
| crashed.steps[3] | From now on press Ctrl+S every few minutes. | Press Ctrl+S every few minutes as you work. |
| shape-jumped.fix | Press Ctrl+Z. Then drag the shape close to the right size first and dimension it again. | Press Ctrl+Z. Move the shape close to the size you need. Add the size label again. |
| shape-jumped.steps[0] | Press Ctrl+Z to undo the jump. | Press Ctrl+Z once. Check the shape. |
| shape-jumped.steps[1] | Drag the lines so they are already close to the size you want. | Place lines near the size you want before adding sizes. |
| shape-jumped.steps[2] | Dimension the big sizes first, then the small ones. | Set the large sizes first. Set the small sizes next. |
| dim-box.fix | Double-click the dimension number. The box opens. | Double-click the size label to edit its value. |
| dim-box.why | The box opens automatically only when a setting is on. Double-clicking always works. | The automatic value box depends on a setting. A size label can usually be edited by double-clicking it. |
| dim-box.steps[0] | Double-click the number. | Double-click the size number. |
| dim-box.steps[1] | Type the value and press Enter. | Type the value. Press Enter. |
| wrong-plane.fix | Right-click the sketch in the tree, pick Edit Sketch Plane, then click Front Plane. | Exit the sketch. Right-click it in the left tree. Choose Edit Sketch Plane. Select Front Plane. |
| wrong-plane.steps[1] | Pick Edit Sketch Plane. | Click Edit Sketch Plane. |
| wrong-plane.steps[2] | Click Front Plane in the tree, then the green check. | Click Front Plane in the tree. Click the green check. |
| rebuild.fix | Press Ctrl+B. | Press Ctrl+B to update the part. |
| rebuild.steps[0] | Press Ctrl+B to rebuild. | Press Ctrl+B to update the part. |
| rebuild.steps[1] | Still marked? Press Ctrl+Q to rebuild everything. | Press Ctrl+Q if it still needs a full update. |
| dragged-arrow.fix | Press Ctrl+Z. | Press Ctrl+Z once. Check the size again. |
| dragged-arrow.steps[0] | Press Ctrl+Z. | Press Ctrl+Z once. Check the size. |
| dragged-arrow.steps[1] | To change a size on purpose, double-click its number and type it. | Double-click a size number to change it on purpose. Type the new value. Press Enter. |
| new-part.fix | File, New, Part, OK. Then switch to inches and save it with your name. | Click File > New. Choose Part. Click OK. Set inches. Save with your last name. |
| new-part.steps[0] | Click File, then New (or the blank page icon at the top). | Click File > New. |
| new-part.steps[1] | Pick Part and click OK. | Choose Part. Click OK. |
| new-part.steps[2] | Check the bottom-right corner says IPS. If not, click it and pick IPS. | Check for IPS at the bottom-right. Click the unit label to choose IPS if needed. |
| new-part.steps[3] | Save it now: Ctrl+S, name it with your last name. | Press Ctrl+S. Put your last name in the file name. Click Save. |
| set-units.fix | Click the units at the bottom-right corner and pick IPS. | Click the units in the bottom-right corner. Choose IPS. |
| set-units.steps[0] | Click the unit label at the bottom-right corner of the window. | Click the unit label at the bottom-right. |
| set-units.steps[1] | Pick IPS (inch, pound, second). | Choose IPS, which uses inches. |
| set-units.steps[2] | For three decimals: Options (gear), Document Properties, Units, Length decimals .123. | Click Options, the gear icon. Click Document Properties > Units. Set Length decimals to .123. |
| start-sketch.fix | Click Front Plane in the tree, then click the Sketch icon that pops up. | Click Front Plane in the left tree. Click Sketch. |
| start-sketch.why | A sketch is a flat drawing on a plane. The Front Plane is the one you look straight at, so the hook outline goes there. | A sketch is a flat drawing. A plane is a flat guide. This lesson uses Front Plane. |
| start-sketch.steps[0] | In the tree on the left, click Front Plane. | Click Front Plane in the left tree. |
| start-sketch.steps[1] | Click the Sketch icon in the small bar that pops up. Or use the Sketch tab, then Sketch. | Click Sketch on the small bar or the top Sketch tab. |
| start-sketch.steps[2] | The view turns to face the plane. If it does not, press Ctrl+8. | Press Ctrl+8 if the view does not face the sketch. |
| start-sketch.steps[3] | The origin (two small red arrows) is the center of your part. Start your first circle there. | Find the origin, the red starting point. Start your first circle there. |
| draw.fix | Sketch tab: Line, Circle, or an Arc. Click to start, click to end, Esc to stop. | Choose Line, Circle, or Arc on the Sketch tab. Click to draw. Press Esc to stop. |
| draw.steps[0] | Circle: on the Sketch tab click Circle, click the center, move out, click again. | Click Circle on the Sketch tab. Click the center. Move outward. Click to set the edge. |
| draw.steps[1] | Line: click Line, click a start point, click each corner. Double-click or press Esc to stop. | Click Line. Click a start point. Click each corner. Press Esc to stop. |
| draw.steps[2] | Curve that flows from a line: while drawing a line, move the mouse back onto the last point and out again. It switches to an arc. Or use Tangent Arc. | Click Tangent Arc for a smooth curve from a line. Click the line end to start it. |
| draw.steps[3] | Start and end points should snap onto each other. An orange square means they joined. | Check that the ends join where you want a closed outline. |
| smart-dim.fix | Sketch tab, Smart Dimension, click the thing, click to place, type the number, Enter. | Click Smart Dimension on the Sketch tab. Click an item. Place the label. Type the size. Press Enter. |
| smart-dim.steps[0] | Click Smart Dimension on the Sketch tab (the icon with a ruler and a diameter sign). | Click Smart Dimension on the Sketch tab. |
| smart-dim.steps[1] | Line length: click the line. Circle diameter: click the circle. Center to center: click one circle, then the other. | Click a line for its length or a circle for its full width. Click two circles for the space between their centers. |
| smart-dim.steps[2] | Move the mouse to where the number should sit and click. | Move the pointer to place the size label. Click. |
| smart-dim.steps[3] | Type the value in inches and press Enter. | Type the value in inches. Press Enter. |
| smart-dim.steps[4] | Press Esc when you are done dimensioning. | Press Esc when you finish adding sizes. |
| change-dim.fix | Double-click the part to show its numbers, double-click the number, type the new value, press Enter. | Double-click the part to show its sizes. Double-click a size label. Type the new value. Press Enter. |
| change-dim.steps[0] | Double-click the feature in the tree, or on the part. Its dimensions appear. | Double-click the feature in the tree. Its size labels appear. |
| change-dim.steps[2] | Type the new value and press Enter. | Type the new value. Press Enter. |
| change-dim.steps[3] | Press Ctrl+B to rebuild. | Press Ctrl+B to update the shape. |
| exit-sketch.fix | Click the Exit Sketch icon in the top-right corner of the part area. | Click Exit Sketch on the Sketch tab. |
| exit-sketch.steps[0] | Look at the top-right corner of the part area. | Find Exit Sketch on the Sketch tab. |
| exit-sketch.steps[1] | Click the icon with the sketch and the check mark to keep your sketch. Do NOT click the red X: it throws away your changes. | Click Exit Sketch to finish drawing. |
| extrude.fix | Features tab, Extruded Boss/Base, Mid Plane, type the thickness, green check. | Click Extruded Boss/Base on the Features tab. Choose Mid Plane. Type the depth. Click the green check. |
| extrude.why | An extrude pushes a closed outline straight out to make a solid. Mid Plane keeps it centered. | Extruded Boss/Base adds depth to a closed outline. Mid Plane adds equal depth on each side. |
| extrude.steps[1] | Click Extruded Boss/Base. | Click Extruded Boss/Base to give the sketch thickness. |
| extrude.steps[2] | If it asks for a sketch, click your sketch. | Click your sketch if asked to choose one. |
| extrude.steps[3] | Under Direction 1 pick Mid Plane and type the thickness (0.750 is the most allowed). | Choose Mid Plane under Direction 1. Type the thickness your assignment asks for. |
| sketch-face.fix | Click a flat face of the part, then click the Sketch icon that pops up. | Click a flat face on the part. Click Sketch. |
| sketch-face.steps[0] | Click a flat face. It turns blue. | Click a flat face on the part. |
| sketch-face.steps[1] | Click Sketch in the bar that pops up. | Click Sketch on the small bar. |
| sketch-face.steps[2] | Press Ctrl+8 to look straight at it. | Press Ctrl+8 to face the sketch. |
| sketch-face.steps[3] | Draw a closed outline, then use Extruded Boss/Base to add or Extruded Cut to remove. | Draw a closed outline. Use Extruded Boss/Base to add material or Extruded Cut to remove it. |
| cut.fix | Sketch the shape on a face, then Features tab, Extruded Cut, set the depth, green check. | Draw a closed shape on a flat face. Exit the sketch. Click Extruded Cut on the Features tab. Set the depth. Click the green check. |
| cut.steps[0] | Sketch a closed shape on a face. | Draw a closed outline in a sketch on the face. |
| cut.steps[1] | Features tab, Extruded Cut. | Click Features > Extruded Cut. |
| cut.steps[2] | Pick Through All for a hole all the way through, or Blind and type a depth for a pocket. | Choose Through All for a hole through the part. For a pocket, choose Blind. Type its depth. |
| cut.steps[3] | Nothing cut? Click the arrow next to the depth to flip the direction. | Check the preview. Click the direction arrow if the cut points away from the part. |
| fillet.fix | Features tab, Fillet, type a radius, click the edges, green check. | Click Fillet on the Features tab. Type a small radius. Select the edges. Click the green check. |
| fillet.why | Sharp inside corners are where cracks start. Rounding them makes the hook stronger. | A fillet rounds an edge. Radius sets how large that round is. |
| fillet.steps[0] | Features tab, Fillet. | Click Features > Fillet to round an edge. |
| fillet.steps[1] | Type a radius, like 0.060. | Type the radius, the size of the round. For example, try 0.060 inches. |
| fillet.steps[2] | Click each edge to round. They list on the left. | Click the edges you want to round. |
| fillet.steps[3] | Green check. If it fails, make the radius smaller. | Click the green check. Try a smaller radius if it fails. |
| chamfer.fix | Features tab, click the small arrow under Fillet, Chamfer, type a distance, click edges, green check. | Open the arrow next to Fillet. Click Chamfer. Type a distance. Select the edges. Click the green check. |
| chamfer.why | A 45 degree chamfer on an edge that faces down on the printer prints without support. | A chamfer is a flat bevel. Some downward-facing bevels need less print support; check the printer settings. |
| chamfer.steps[0] | Features tab. Click the small arrow under Fillet and pick Chamfer. | Open the arrow under Fillet on the Features tab. Click Chamfer to make a sloped edge. |
| chamfer.steps[1] | Type a distance and click the edges. | Type a distance. Click the edges. |
| chamfer.steps[2] | Green check. | Click the green check. |
| mirror.fix | Features tab, Mirror, pick Front Plane as the mirror, pick the feature to copy, green check. | Click Mirror on the Features tab. Choose Front Plane. Pick the step to copy. Click the green check. |
| mirror.steps[0] | Features tab, Mirror. | Click Features > Mirror to copy across a plane. |
| mirror.steps[1] | Mirror Face/Plane: click Front Plane in the tree (click the small arrow at the top of the left panel to show it). | Click the Mirror Face/Plane box. Click Front Plane in the tree. |
| mirror.steps[2] | Features to Mirror: click the cut or boss you made on one side. | Click the Features to Mirror box. Click the cut or boss to copy. |
| mirror.steps[3] | Green check. In a sketch, use Mirror Entities with a centerline instead. | Click the green check. Use Mirror Entities for lines inside a sketch. |
| offset.fix | Sketch tab, Offset Entities, type a distance, click the edges, flip inward if needed, green check. | Click Offset Entities on the Sketch tab. Type the gap size. Select the edges. Check the direction. Click the green check. |
| offset.why | An offset keeps an even wall, which is how you make a raised rim: offset inward, then cut the pocket. | Offset Entities copies edges at a fixed gap. It helps make walls with an even width. |
| offset.steps[1] | Sketch tab, Offset Entities. | Click Sketch > Offset Entities to copy edges nearby. |
| offset.steps[2] | Type a distance and click the outer edges. | Type a distance. Click the edges to copy. |
| offset.steps[3] | Check Reverse if it went outward. Green check. | Check Reverse if the copy goes the wrong way. Click the green check. |
| trim.fix | Sketch tab, Trim Entities, Power trim, drag across the pieces you do not want. | Click Trim Entities on the Sketch tab. Choose Power trim. Drag across only the pieces you want to remove. |
| trim.steps[0] | Sketch tab, Trim Entities. | Click Sketch > Trim Entities. |
| trim.steps[1] | Pick Power trim. | Choose Power trim. |
| trim.steps[2] | Hold the left mouse button and drag across the pieces. They disappear. | Drag across unwanted pieces with the left mouse button held down. |
| trim.steps[3] | Esc when done. | Press Esc when finished. |
| centerline.fix | Sketch tab, click the small arrow next to Line, Centerline. | Open the arrow next to Line on the Sketch tab. Click Centerline. |
| centerline.why | A centerline is a guide. It never becomes plastic, and Mirror Entities uses it as the mirror. | A centerline is a drawing guide. It does not become part of the solid shape. |
| centerline.steps[0] | Click the arrow next to Line and pick Centerline. | Open the arrow next to Line. Click Centerline, a guide for your drawing. |
| centerline.steps[1] | Draw it like a normal line. | Click a start point. Click an end point. Press Esc. |
| relations.fix | Hold Ctrl and click both items, then pick the relation on the left. | Hold Ctrl while clicking two items. Choose a drawing rule in the left panel. |
| relations.why | Relations lock how shapes connect. Tangent makes an arc flow into a line with no corner, which is stronger. | Relations are rules between drawing items. Tangent joins a curve smoothly, without a sharp corner. |
| relations.steps[0] | Hold Ctrl and click the two items. | Select two items while holding Ctrl. |
| relations.steps[1] | On the left, under Add Relations, pick one: Tangent (smooth join), Coincident (touching), Horizontal, Equal. | Choose a rule under Add Relations on the left. Tangent means a smooth join. Coincident means touching. Equal means the same size. Horizontal means level. |
| relations.steps[2] | Press Esc. | Press Esc when finished. |
| measure.fix | Evaluate tab, Measure, click what you want to measure. | Click Measure on the Evaluate tab. Click the items you want to check. |
| measure.steps[0] | Click the Evaluate tab, then Measure. | Click Evaluate > Measure. |
| measure.steps[1] | Two circle edges: shows the center to center distance. | Select two circle edges to check their center distance. |
| measure.steps[2] | One circle edge: shows its diameter. | Select one circle edge to check its full width. |
| measure.steps[3] | Two faces: shows the distance between them, like the thickness. | Select two faces to check the distance between them. |
| measure.steps[4] | Click in empty space to clear, and close the box when done. | Click empty space to clear the selection. Close Measure when finished. |
| move-view.fix | Hold the middle mouse button to turn. Scroll to zoom. Ctrl + middle button to slide. | Hold the middle mouse button to turn the view. Scroll to zoom. Hold Ctrl and the middle button to slide the view. |
| move-view.steps[0] | Turn: hold the middle mouse button (the scroll wheel) and drag. | Drag with the middle mouse button held down to turn the view. |
| move-view.steps[1] | Zoom: roll the scroll wheel. | Roll the scroll wheel to zoom. |
| move-view.steps[2] | Slide: hold Ctrl and the middle mouse button and drag. | Drag while holding Ctrl and the middle mouse button to slide the view. |
| move-view.steps[3] | Fit everything: F. Pick a view: spacebar. | Press F to fit the part. Press the spacebar to choose a view. |
| undo.fix | Ctrl+Z. Ctrl+Y puts it back. | Press Ctrl+Z to undo one change. Press Ctrl+Y to redo it. |
| undo.why | Undo is always safer than deleting. | Undo can reverse a recent change. Check the result; not every view or editing action can be undone. |
| undo.steps[0] | Press Ctrl+Z. Each press undoes one step. | Press Ctrl+Z once. Check what changed. |
| undo.steps[1] | Went too far? Ctrl+Y redoes. | Press Ctrl+Y if you need to redo the change. |
| edit-feature.fix | Right-click the feature in the tree and pick Edit Feature. | Right-click a step in the left tree. Click Edit Feature. |
| edit-feature.steps[0] | Right-click the feature in the tree. | Right-click the feature, a step in your part. |
| edit-feature.steps[1] | Pick Edit Feature (the icon with a pencil and a box). | Click Edit Feature. |
| edit-feature.steps[2] | Change the settings and click the green check. | Change a setting. Click the green check. |
| check-sketch.fix | Tools menu, Sketch Tools, Check Sketch for Feature, then Check. | Click Tools > Sketch Tools > Check Sketch for Feature. Choose the tool you plan to use. Click Check. |
| check-sketch.steps[0] | While in the sketch, open the Tools menu. | Open the Tools menu while editing the sketch. |
| check-sketch.steps[1] | Sketch Tools, then Check Sketch for Feature. | Click Sketch Tools > Check Sketch for Feature. |
| check-sketch.steps[2] | Click Check. It lists and highlights the problems. | Click Check. Read the result. |
| save.fix | File, Save As, type the name you were given with your last name, Save. | Click File > Save As. Type a name with your last name. Choose your class folder. Click Save. |
| save.why | The .SLDPRT file is what you hand in. A drawing, a STEP or a screenshot is not the part. | Save a SOLIDWORKS part as .SLDPRT. Keep it in your class folder so you can edit it later. |
| save.steps[0] | Click File, then Save As. | Click File > Save As. |
| save.steps[1] | Type the name your assignment asks for, like V1_SMITH. | Type the name your assignment asks for, such as V1_SMITH. |
| save.steps[2] | Save as type should be Part (*.sldprt). | Choose Part (*.sldprt) in Save as type. |
| save.steps[3] | Save it where Mr. Pina told the class. | Choose your class folder. Click Save. |
| save.steps[4] | After that, Ctrl+S saves again. | Press Ctrl+S to save later changes. |
| hide-planes.fix | View menu, Hide/Show, Planes. | Click View > Hide/Show > Planes. |
| hide-planes.steps[0] | Click View, then Hide/Show, then Planes. | Click View > Hide/Show > Planes to show or hide the flat drawing guides. |
| IdeaUnstuck.Core/Rules.cs | "Click File > Open and choose your .SLDPRT." | "Click File > Open. Choose your .SLDPRT part file." |
| IdeaUnstuck.Core/Rules.cs | "Features below it are switched off." | "Features are steps in your part. Steps below the bar are switched off." |
| IdeaUnstuck.Core/Rules.cs | " in the left tree and choose Edit Feature." | " in the left tree. Click Edit Feature." |
| IdeaUnstuck.Core/Rules.cs | "A solid extrusion needs a closed outline." | "Extrude gives a sketch thickness. This lesson needs a closed outline." |
| IdeaUnstuck.Core/Rules.cs | "Right-click the last Extrude in the left tree and choose Edit Feature." | "Right-click the last Extrude in the left tree. Click Edit Feature." |
| IdeaUnstuck.Core/Rules.cs | "Press Ctrl+S and name it with your last name." | "Press Ctrl+S. Put your last name in the file name." |
| IdeaUnstuck.Core/Rules.cs | ":"; ":" | "\|" |
| IdeaUnstuck.Core/Rules.cs | "Looks like you are stuck."; "You can ask for help with your exact situation."; "Press Copy for AI and paste it into your AI chat with a screenshot, or raise your hand for a TA." | "Need a hand?"; "You stopped the same tool three times. Try its More help steps."; "You used Undo several times. Pause and check the last change."; "This help card has stayed the same for two minutes. Try its next step."; "Press Esc. Read More help. You can also press Copy for AI or ask a TA." |
| IdeaUnstuck.Core/Rules.cs | "five_undos"; "three_minutes" | "three_cancels"; "four_undos"; "two_minutes" |
| IdeaUnstuck.AddIn/Addin.cs | (new) | "subscribe.CommandOpenPreNotify"; "subscribe.CommandCloseNotify" |
| IdeaUnstuck.AddIn/Addin.cs | (new) | "unsubscribe.CommandOpenPreNotify"; "unsubscribe.CommandCloseNotify" |
| IdeaUnstuck.AddIn/Addin.cs | (new) | "CommandOpened"; "CommandClosed" |
| IdeaUnstuck.AddIn/Addin.cs | "Now press Windows+Shift+S, drag over the whole SOLIDWORKS window, and paste it into the same chat." | "Press Windows+Shift+S. Drag over the whole SOLIDWORKS window. Paste the screenshot into the same chat." |
| IdeaUnstuck.Core/ErrorWords.cs | "The extrusion cannot find its ending face."; "These extrusion settings cannot make a solid shape." | "Extrude adds thickness to a sketch. It cannot find its ending face."; "Extrude adds thickness to a sketch. These settings cannot make a solid shape." |
| IdeaUnstuck.Core/ErrorWords.cs | "The sketch outline cannot make this extrusion." | "Extrude adds thickness to a sketch. This outline cannot make that shape." |
| IdeaUnstuck.Core/ErrorWords.cs | "The partial rounding does not meet its reference." | "The partial round does not meet the item it uses as a guide." |
| IdeaUnstuck.Core/ErrorWords.cs | "The sheet metal sweep needs a valid outline and path."; "The sheet metal sweep crosses itself." | "Sweep moves an outline along a path. Check both the outline and path."; "Sweep moves an outline along a path. This shape crosses itself." |
| IdeaUnstuck.Core/ErrorWords.cs | "The outlines are nested more than this tool allows." | "There are too many outlines inside other outlines for this tool." |
| IdeaUnstuck.Core/GuidedPaths.cs | "Select Front Plane, then click Sketch." | "Click Front Plane. Click Sketch." |
| IdeaUnstuck.Core/GuidedPaths.cs | "Click Smart Dimension. Click a line, place the size, then type its value." | "Click Smart Dimension. Click a line. Place the size label. Type a value." |
| IdeaUnstuck.Core/GuidedPaths.cs | "Click Features > Extruded Boss/Base. Choose Mid Plane, then click the green check." | "Click Features > Extruded Boss/Base. Choose Mid Plane. Click the green check." |
| IdeaUnstuck.Core/GuidedPaths.cs | "Double-click a size. Type a new value and press Enter." | "Double-click a size. Type a new value. Press Enter." |
| IdeaUnstuck.Core/GuidedPaths.cs | "Click a flat face, then click Sketch." | "Click a flat face. Click Sketch." |
| IdeaUnstuck.Core/GuidedPaths.cs | "Click Exit Sketch, then Features > Extruded Cut. Click the green check." | "Click Exit Sketch. Click Features > Extruded Cut. Click the green check." |
| IdeaUnstuck.Core/GuidedPaths.cs | "Click Features > Fillet. Pick an edge, type a radius, then click the green check." | "Click Features > Fillet. Pick an edge. Type a radius: the size of the round. Click the green check." |
| IdeaUnstuck.Core/GuidedPaths.cs | (new) | "First 10 minutes"; "e1"; "Click File > New > Part."; "Top-left menu: File > New. Choose Part."; "e2"; "Check that the units are inches."; "units"; "Bottom-right status bar: look for IPS. In this pane, Set units to inches changes it."; "e3"; "Click Front Plane. Click Sketch."; "front"; "Left tree: Front Plane. Top ribbon: Sketch tab, then Sketch."; "e4"; "Click Circle. Start at the origin."; "Top Sketch tab: Circle. In the drawing area, click the red origin point. It is the starting point."; "e5"; "Click Smart Dimension. Set the circle's width to 1.000."; "Top Sketch tab: Smart Dimension. Click the circle edge. Place the label. Type 1.000. Press Enter. The diameter symbol means full width."; "e6"; "Click Exit Sketch."; "exit"; "Top-left Sketch tab: Exit Sketch. This pane also has Exit sketch."; "e7"; "Click Extruded Boss/Base. Choose Mid Plane. Set 0.500. Click the green check."; "Top Features tab: Extruded Boss/Base. Left settings panel: Direction 1, Mid Plane, depth 0.500 inches."; "e8"; "Press Ctrl+S. Put your last name in the file name."; "Keyboard: hold Ctrl and press S. The save window lets you choose your class folder and name." |
| IdeaUnstuck.Core/GuidedPaths.cs | (new) | "e1"; "e2"; "e3"; "plane"; "Front Plane"; "e4"; "e3"; "e5"; "e4"; "e6"; "e5"; "e7"; "e6"; "e8"; "e7" |
