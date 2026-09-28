@echo off
setlocal
%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Setup.ps1" -Mode Install
if errorlevel 1 (
  echo FAILURE: IDEA Unstuck was not installed. Read the message above.
  exit /b 1
)
echo SUCCESS: IDEA Unstuck is installed for this PC.
exit /b 0
