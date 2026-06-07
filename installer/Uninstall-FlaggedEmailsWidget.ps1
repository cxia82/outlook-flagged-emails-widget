param(
    [string]$InstallDir = (Join-Path $env:LOCALAPPDATA "Programs\FlaggedEmailsWidget")
)

$ErrorActionPreference = "Stop"

Get-Process -Name "NotificationWidget" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

$desktop = [Environment]::GetFolderPath("Desktop")
$programs = [Environment]::GetFolderPath("Programs")
$shortcutName = "Flagged Emails.lnk"
$desktopShortcutPath = Join-Path $desktop $shortcutName
$startMenuFolder = Join-Path $programs "Flagged Emails Widget"
$startMenuShortcutPath = Join-Path $startMenuFolder $shortcutName

if (Test-Path $desktopShortcutPath) { Remove-Item $desktopShortcutPath -Force }
if (Test-Path $startMenuShortcutPath) { Remove-Item $startMenuShortcutPath -Force }
if (Test-Path $startMenuFolder) { Remove-Item $startMenuFolder -Recurse -Force }

Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "FlaggedEmailsWidget" -ErrorAction SilentlyContinue
Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" -Recurse -Force -ErrorAction SilentlyContinue

if (Test-Path $InstallDir) {
    Remove-Item $InstallDir -Recurse -Force
}

Write-Host "Uninstall complete"
