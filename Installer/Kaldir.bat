@echo off
chcp 65001 >nul
echo A4Tech Control kaldiriliyor...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Uninstall.ps1"
echo.
pause
