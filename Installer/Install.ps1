<#
    A4Tech Control — kullanıcı seviyesinde kurulum (admin gerekmez).
    Kullanım: powershell -ExecutionPolicy Bypass -File .\Install.ps1
#>
param(
    [string]$Source = (Join-Path $PSScriptRoot '..\bin\Release\net8.0-windows\win-x64\publish'),
    [string]$InstallDir = (Join-Path $env:LOCALAPPDATA 'Programs\A4TechControl'),
    [switch]$NoLaunch,
    [switch]$NoStartMenu
)

$ErrorActionPreference = 'Stop'
$sourcePath = (Resolve-Path -LiteralPath $Source).Path
$sourceExe = Join-Path $sourcePath 'A4TechControl.exe'

if (-not (Test-Path -LiteralPath $sourceExe)) {
    throw "Kaynak exe bulunamadi: $sourceExe  -- once 'dotnet publish' calistirin."
}

Write-Host 'A4Tech Control kuruluyor...' -ForegroundColor Cyan

# Calisan kopya varsa kapat (dosya kilitli kalmasin).
Get-Process -Name A4TechControl -ErrorAction SilentlyContinue | Stop-Process -Force

New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
Get-ChildItem -LiteralPath $sourcePath -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $InstallDir -Force
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.ps1') -Destination $InstallDir -Force

$exe = Join-Path $InstallDir 'A4TechControl.exe'

# Acilista baslat ayari varsa yeni kurulum yoluna guncelle (kapaliysa dokunma).
$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$existingRun = (Get-ItemProperty -Path $runKey -Name 'A4TechControl' -ErrorAction SilentlyContinue).A4TechControl
if ($existingRun) {
    Set-ItemProperty -Path $runKey -Name 'A4TechControl' -Value "`"$exe`""
}

# Baslat menusu kisayolu
if (-not $NoStartMenu) {
    $startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
    New-Item -ItemType Directory -Path $startMenu -Force | Out-Null
    $shortcut = Join-Path $startMenu 'A4Tech Control.lnk'
    $shell = New-Object -ComObject WScript.Shell
    $link = $shell.CreateShortcut($shortcut)
    $link.TargetPath = $exe
    $link.WorkingDirectory = $InstallDir
    $link.IconLocation = "$exe,0"
    $link.Description = 'FG35 / FGK10 fare ve klavye yardimcisi'
    $link.Save()
}

# Program Ekle/Kaldir kaydi (HKCU -> admin gerekmez)
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\A4TechControl'
New-Item -Path $uninstallKey -Force | Out-Null
$sizeKb = [math]::Round(((Get-ChildItem -LiteralPath $InstallDir -File | Measure-Object Length -Sum).Sum) / 1KB)
Set-ItemProperty -Path $uninstallKey -Name DisplayName -Value 'A4Tech Control'
Set-ItemProperty -Path $uninstallKey -Name DisplayVersion -Value '1.0.8'
Set-ItemProperty -Path $uninstallKey -Name Publisher -Value 'A4Tech Control'
Set-ItemProperty -Path $uninstallKey -Name DisplayIcon -Value $exe
Set-ItemProperty -Path $uninstallKey -Name InstallLocation -Value $InstallDir
Set-ItemProperty -Path $uninstallKey -Name EstimatedSize -Value $sizeKb
Set-ItemProperty -Path $uninstallKey -Name UninstallString -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$(Join-Path $InstallDir 'Uninstall.ps1')`""

if (-not $NoLaunch) {
    Start-Process -FilePath $exe
}

Write-Host "Kurulum tamamlandi: $InstallDir" -ForegroundColor Green
Write-Host 'Baslat menusunde A4Tech Control kisayolu olusturuldu.' -ForegroundColor Green
