<#
    A4Tech Companion — kaldirma.
    Kullanım: powershell -ExecutionPolicy Bypass -File .\Uninstall.ps1
#>
param([switch]$RemoveSettings)

$ErrorActionPreference = 'SilentlyContinue'
$installDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host 'A4Tech Companion kaldiriliyor...' -ForegroundColor Cyan

Get-Process -Name A4TechCompanion -ErrorAction SilentlyContinue | Stop-Process -Force

$shortcut = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\A4Tech Companion.lnk'
Remove-Item -LiteralPath $shortcut -Force

Remove-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\A4TechCompanion' -Recurse -Force
Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'A4TechCompanion' -Force

if ($RemoveSettings) {
    Remove-Item -Path (Join-Path $env:APPDATA 'A4TechCompanion') -Recurse -Force
}

# Bu betigin kendisi de klasorun icinde; bir sonraki oturumda temizlik yapilir.
Start-Process -FilePath 'cmd.exe' -ArgumentList "/c ping -n 2 127.0.0.1 >nul & rmdir /s /q `"$installDir`"" -WindowStyle Hidden

Write-Host 'Kaldirma tamamlandi.' -ForegroundColor Green
