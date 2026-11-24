@echo off
chcp 65001 >nul
echo A4Tech Companion kurulumu baslatiliyor...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
echo.
pause
