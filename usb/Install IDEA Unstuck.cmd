@echo off
rem IDEA Unstuck 0.3.0: installs for every Windows user on this PC
rem Double-click to run. Works offline. Keep this file next to the "files" folder.
setlocal
set "HERE=%~dp0"
set "QUIET="
if /i "%~1"=="/quiet" set "QUIET=1"
fltmc >nul 2>&1
if errorlevel 1 (
  if defined QUIET (echo FAIL: run this as administrator. & exit /b 1)
  echo Asking Windows for administrator rights. Choose Yes.
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "try { Start-Process -FilePath '%~f0' -ArgumentList 'elevated' -Verb RunAs -ErrorAction Stop } catch { exit 5 }"
  if errorlevel 5 (
    echo.
    echo   FAIL   Administrator rights were not given, so nothing was changed.
    echo.
    pause
    exit /b 1
  )
  exit /b 0
)
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%HERE%files\Setup.ps1" -Mode Install -Source "%HERE%files" -LogDir "%HERE%logs"
set "RESULT=%errorlevel%"
if not defined QUIET pause
exit /b %RESULT%
