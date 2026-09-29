IDEA Unstuck 0.3.0 - flash drive installer
==========================================

IDEA Unstuck adds a help panel to SOLIDWORKS 2025 and 2026 for students.
It works offline. It needs no account and no internet.


A. PUT IT ON THE FLASH DRIVE (once)
-----------------------------------
1. Copy IDEA-Unstuck-USB-v0.3.0.zip onto the flash drive.
2. Right-click the ZIP on the flash drive. Click "Extract All...".
3. Click "Extract". A folder named IDEA-Unstuck-USB-v0.3.0 appears.
   Open it. You should see:
      Install IDEA Unstuck.cmd
      Uninstall IDEA Unstuck.cmd
      Check IDEA Unstuck.cmd
      README.txt
      files   (folder)
      logs    (folder)
   Do not run anything from inside the ZIP itself.


B. INSTALL ON ONE COMPUTER (about 30 seconds)
---------------------------------------------
1. Close SOLIDWORKS on that computer.
2. Plug in the flash drive. Open the IDEA-Unstuck-USB-v0.3.0 folder.
3. Double-click "Install IDEA Unstuck.cmd".
4. Windows asks "Do you want to allow this app to make changes?".
   Click Yes. (If it asks for a password, use an administrator account.)
5. Wait for a large green line that starts with PASS.
   Press any key to close the window.
   - A red line that starts with FAIL tells you why. Fix that one thing
     (for example, close SOLIDWORKS) and double-click Install again.
6. Nothing restarts. Every Windows user on this computer, including a
   student who has never signed in before, gets the help panel the next
   time they open SOLIDWORKS. No box to tick.

Running Install again is safe. It also upgrades version 0.2.


C. SEE IT IN SOLIDWORKS
-----------------------
1. Open SOLIDWORKS.
2. On the far right edge are small tab icons. Click the green magnifier
   with a small wrench. That is IDEA Unstuck.
   (No tabs on the right? Click View > User Interface > Task Pane.)


D. COUNT THE COMPUTERS YOU HAVE DONE
------------------------------------
Open the "logs" folder on the flash drive. Each computer gets its own file,
named after the computer, with one line for every run and PASS or FAIL.


E. CHECK OR REMOVE
------------------
- "Check IDEA Unstuck.cmd" shows the installed version and which
  SOLIDWORKS versions it covers. It changes nothing. No password needed.
- "Uninstall IDEA Unstuck.cmd" removes it. Students' parts are kept.
  It can also be removed from Settings > Apps > IDEA Unstuck.
