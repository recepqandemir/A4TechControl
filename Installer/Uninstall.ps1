<#
    A4Tech Control — kaldirma.
    Kullanım: powershell -ExecutionPolicy Bypass -File .\Uninstall.ps1
#>
param([switch]$RemoveSettings)

$ErrorActionPreference = 'SilentlyContinue'
$installDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host 'A4Tech Control kaldiriliyor...' -ForegroundColor Cyan

Get-Process -Name A4TechControl -ErrorAction SilentlyContinue | Stop-Process -Force

$shortcut = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\A4Tech Control.lnk'
Remove-Item -LiteralPath $shortcut -Force

Remove-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\A4TechControl' -Recurse -Force
Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'A4TechControl' -Force

if ($RemoveSettings) {
    Remove-Item -Path (Join-Path $env:APPDATA 'A4TechControl') -Recurse -Force
}

# Bu betigin kendisi de klasorun icinde; bir sonraki oturumda temizlik yapilir.
Start-Process -FilePath 'cmd.exe' -ArgumentList "/c ping -n 2 127.0.0.1 >nul & rmdir /s /q `"$installDir`"" -WindowStyle Hidden

Write-Host 'Kaldirma tamamlandi.' -ForegroundColor Green
