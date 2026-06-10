param(
    [string]$InstallDir = (Join-Path $env:LOCALAPPDATA "Programs\FlaggedEmailsWidget")
)

$ErrorActionPreference = "Stop"

Get-Process -Name "NotificationWidget" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

$desktop = [Environment]::GetFolderPath("Desktop")
$programs = [Environment]::GetFolderPath("Programs")
$startup = [Environment]::GetFolderPath("Startup")
$shortcutName = "Flagged Emails.lnk"
$startupShortcutName = "Flagged Emails Widget.lnk"
$desktopShortcutPath = Join-Path $desktop $shortcutName
$startMenuFolder = Join-Path $programs "Flagged Emails Widget"
$startMenuShortcutPath = Join-Path $startMenuFolder $shortcutName
$startupShortcutPath = Join-Path $startup $startupShortcutName

if (Test-Path $desktopShortcutPath) { Remove-Item $desktopShortcutPath -Force }
if (Test-Path $startMenuShortcutPath) { Remove-Item $startMenuShortcutPath -Force }
if (Test-Path $startMenuFolder) { Remove-Item $startMenuFolder -Recurse -Force }
if (Test-Path $startupShortcutPath) { Remove-Item $startupShortcutPath -Force }

Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "FlaggedEmailsWidget" -ErrorAction SilentlyContinue
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run" -Name "FlaggedEmailsWidget" -ErrorAction SilentlyContinue
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32" -Name "FlaggedEmailsWidget" -ErrorAction SilentlyContinue
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder" -Name $startupShortcutName -ErrorAction SilentlyContinue
Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" -Recurse -Force -ErrorAction SilentlyContinue

if (Test-Path $InstallDir) {
    Remove-Item $InstallDir -Recurse -Force
}

Write-Host "Uninstall complete"
