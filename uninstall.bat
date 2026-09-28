@echo off
setlocal
%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Setup.ps1" -Mode Uninstall
if errorlevel 1 (
  echo FAILURE: IDEA Unstuck was not completely uninstalled. Read the message above.
  exit /b 1
)
echo SUCCESS: IDEA Unstuck is uninstalled. Your parts and teaching logs were kept.
exit /b 0
