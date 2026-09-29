@echo off
rem Installed copy used by Settings > Apps > IDEA Unstuck > Uninstall.
rem It runs setup from a temporary copy, because the uninstall deletes this folder.
setlocal
fltmc >nul 2>&1
if errorlevel 1 (
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "try { Start-Process -FilePath '%~f0' -Verb RunAs -ErrorAction Stop } catch { exit 5 }"
  exit /b 0
)
set "TMPSETUP=%TEMP%\IdeaUnstuckSetup-%RANDOM%.ps1"
copy /y "%~dp0Setup.ps1" "%TMPSETUP%" >nul
cd /d "%TEMP%"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%TMPSETUP%" -Mode Uninstall
set "RESULT=%errorlevel%"
del "%TMPSETUP%" >nul 2>&1
pause
exit /b %RESULT%
