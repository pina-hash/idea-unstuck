# Five-minute check for IDEA Unstuck v0.3.0

Do part A tonight on your SOLIDWORKS 2026 laptop. Do part B tomorrow on **one** Windows 10 lab computer with SOLIDWORKS 2025, before any other lab computer. Only when B passes, do the rest of the lab with part C.

You need: the release ZIP `IDEA-Unstuck-USB-v0.3.0.zip` from https://github.com/pina-hash/idea-unstuck/releases/tag/v0.3.0 and a flash drive.

## Put it on the flash drive (once)

1. Download `IDEA-Unstuck-USB-v0.3.0.zip` from the release page above.
2. Copy it onto the flash drive.
3. On the flash drive, right-click the ZIP. Click **Extract All...**. Click **Extract**.
   - You should see: a folder `IDEA-Unstuck-USB-v0.3.0` holding `Install IDEA Unstuck.cmd`, `Uninstall IDEA Unstuck.cmd`, `Check IDEA Unstuck.cmd`, `README.txt`, and the folders `files` and `logs`.
   - If different: you opened the ZIP instead of extracting it. Close the window and repeat step 3.

## A. Tonight, SOLIDWORKS 2026 laptop

1. Close SOLIDWORKS. On the flash drive, open `IDEA-Unstuck-USB-v0.3.0`. Double-click **Install IDEA Unstuck.cmd**. Click **Yes** in the Windows box.
   - You should see: a black window, then within about 30 seconds a large green line starting **PASS** that says "Upgraded 0.x to 0.3.0 for SOLIDWORKS 2026" (or "Installed 0.3.0 ..."). Press any key.
   - If you see a red **FAIL** line: it names the problem, for example "SOLIDWORKS is open". Fix that one thing and run it again. If the reason makes no sense, take a photo of the window.
2. Open SOLIDWORKS 2026. Look at the far right edge of the window for the column of small task pane tabs.
   - You should see: a dark square with a green magnifier holding a small wrench. Click it. The pane opens with a dark title bar reading **IDEA Unstuck**, **A A+ A++** on the right, and **Help | Practice** tabs under it.
   - If there is no new tab: click **Tools > Add-Ins**. Find **IDEA Unstuck**. Tick both boxes and click **OK**. If it is not in that list at all, run **Check IDEA Unstuck.cmd** and photograph the result.
   - If the tab is there but blank, with no icon: the pane still works. Note it for me. The icon call is the one part of v0.3 no test could run (see the report).
3. Click **File > New > Part**, then **OK**. Click once in the search box that says "Type what's happening, in your own words". Type **my lines are red**.
   - You should see: while you type, up to five rows appear under the box. The first says **Something is wrong** above **My sketch turned red or yellow, or says Over Defined**. Press **Enter**. The list is replaced by that answer: a bold fix, numbered steps with a gray tag such as "Left panel" or "Top ribbon", then **Did that fix it?** with **Yes** and **Not yet**.
   - Also check: press **Esc** in the search box. The box empties. Now press **S** with the mouse over the part area: the SOLIDWORKS shortcut bar should pop up. If it does not, keyboard focus stayed in the pane. Note that for me; the rest still works.
   - If the rows do not appear: note it, and continue.
4. Click the **Practice** tab.
   - You should see: "Practice parts", then **First 10 minutes** open with real check boxes. Any step SOLIDWORKS already shows as done (a new part, inch units) is ticked in green. Under each step is **Show me where**, and under some a small **Do it for me** link.
   - Also check while you are here: in your new part, if the units at the bottom right read **MMGS**, go back to **Help**. A white card with a blue edge should say **Right now** / "You are in millimeters." with a blue **Set units to inches** button.
5. Close SOLIDWORKS. Double-click **Uninstall IDEA Unstuck.cmd**. Click **Yes**.
   - You should see: a green **PASS** line: "Removed IDEA Unstuck 0.3.0. Student parts and help logs were kept."
6. Double-click **Install IDEA Unstuck.cmd** again. Click **Yes**.
   - You should see: green **PASS**, "Installed 0.3.0 for SOLIDWORKS 2026 ...".
   - Then open **Settings > Apps > Installed apps** and search **IDEA**. You should see **IDEA Unstuck**, version 0.3.0, publisher "IDEA, Don Bosco Tech", with the magnifier icon.

If step 2 or step 3 fails, stop here. Do not install on the lab tomorrow; v0.2 stays as it is. Everything else is a note, not a stop.

## B. Tomorrow, before class: one Windows 10 lab computer with SOLIDWORKS 2025

1. Sign in as an administrator or as a student (Windows asks for an admin password in step 2 if needed). Close SOLIDWORKS.
2. Plug in the drive. Double-click **Install IDEA Unstuck.cmd**. Click **Yes** and enter admin details if asked.
   - You should see: green **PASS** "... for SOLIDWORKS 2025 ...". If it lists 2025 and 2026, both are covered.
   - If **FAIL** ".NET Framework 4.8 is missing": that computer needs Windows Update first. Skip it for now.
3. Sign in as a student who has used this computer before (or stay signed in if a student account is already signed in). Open SOLIDWORKS 2025.
   - You should see: the magnifier tab on the right, and the pane working as in A2 and A3. No Tools > Add-Ins step should be needed.
   - If the pane is missing for a student but present for the admin: run **Check IDEA Unstuck.cmd** while signed in as that student. The line "Starts with SOLIDWORKS for you" should read **on**. If it reads "not set yet", sign out and back in once, then check again.
4. Search **i lost my part** and open the first result. Click **Did that fix it? > Not yet**.
   - You should see: related answers, then "Still stuck? Raise your hand."

## C. The rest of the lab

1. On each computer: close SOLIDWORKS, double-click **Install IDEA Unstuck.cmd**, click **Yes**, wait for green **PASS**, press a key.
2. Afterwards, open the drive's `logs` folder. There is one file per computer, named after it. Each line reads `... INSTALL PASS v0.3.0 solidworks=2025 ...`. Count the files to count the computers done. A **FAIL** line says why.

To undo everything on a computer: **Uninstall IDEA Unstuck.cmd**, or **Settings > Apps > IDEA Unstuck > Uninstall**. v0.2's own zip can reinstall v0.2 if you ever need it.
