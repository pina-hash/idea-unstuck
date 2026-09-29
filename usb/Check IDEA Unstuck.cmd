@echo off
rem IDEA Unstuck 0.3.1: checks the installation; changes nothing
rem Double-click to run. Works offline. Keep this file next to the "files" folder.
setlocal
set "HERE=%~dp0"
set "QUIET="
if /i "%~1"=="/quiet" set "QUIET=1"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%HERE%files\Setup.ps1" -Mode Check -Source "%HERE%files" -LogDir "%HERE%logs"
set "RESULT=%errorlevel%"
if not defined QUIET pause
exit /b %RESULT%
