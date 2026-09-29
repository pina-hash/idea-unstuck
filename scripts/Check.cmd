@echo off
rem Installed copy: checks IDEA Unstuck on this PC. Changes nothing.
setlocal
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%~dp0Setup.ps1" -Mode Check
set "RESULT=%errorlevel%"
if /i not "%~1"=="/quiet" pause
exit /b %RESULT%
