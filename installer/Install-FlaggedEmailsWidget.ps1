param(
    [string]$InstallDir = (Join-Path $env:LOCALAPPDATA "Programs\FlaggedEmailsWidget"),
    [switch]$LaunchAfterInstall
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appSourceDir = Join-Path $scriptDir "app"
$exeName = "NotificationWidget.exe"
$exeSourcePath = Join-Path $appSourceDir $exeName

if (-not (Test-Path $exeSourcePath)) {
    throw "Installer payload missing: $exeSourcePath"
}

New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
Copy-Item -Path (Join-Path $appSourceDir "*") -Destination $InstallDir -Recurse -Force

$desktop = [Environment]::GetFolderPath("Desktop")
$programs = [Environment]::GetFolderPath("Programs")
$shortcutName = "Flagged Emails.lnk"
$desktopShortcutPath = Join-Path $desktop $shortcutName
$startMenuFolder = Join-Path $programs "Flagged Emails Widget"
$startMenuShortcutPath = Join-Path $startMenuFolder $shortcutName
$targetPath = Join-Path $InstallDir $exeName
$iconPath = Join-Path $InstallDir "app.ico"

New-Item -ItemType Directory -Path $startMenuFolder -Force | Out-Null

$wsh = New-Object -ComObject WScript.Shell

$desktopShortcut = $wsh.CreateShortcut($desktopShortcutPath)
$desktopShortcut.TargetPath = $targetPath
$desktopShortcut.WorkingDirectory = $InstallDir
$desktopShortcut.Description = "Flagged Emails Widget"
if (Test-Path $iconPath) { $desktopShortcut.IconLocation = "$iconPath,0" }
$desktopShortcut.Save()

$startShortcut = $wsh.CreateShortcut($startMenuShortcutPath)
$startShortcut.TargetPath = $targetPath
$startShortcut.WorkingDirectory = $InstallDir
$startShortcut.Description = "Flagged Emails Widget"
if (Test-Path $iconPath) { $startShortcut.IconLocation = "$iconPath,0" }
$startShortcut.Save()

Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "FlaggedEmailsWidget" -Value ('"' + $targetPath + '"')
New-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run" -Force | Out-Null
Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run" -Name "FlaggedEmailsWidget" -Type Binary -Value ([byte[]](0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00))

$uninstallKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget"
New-Item -Path $uninstallKey -Force | Out-Null
Set-ItemProperty -Path $uninstallKey -Name "DisplayName" -Value "Flagged Emails Widget"
Set-ItemProperty -Path $uninstallKey -Name "Publisher" -Value "outlook-flagged-emails-widget"
Set-ItemProperty -Path $uninstallKey -Name "DisplayVersion" -Value "1.0"
Set-ItemProperty -Path $uninstallKey -Name "InstallLocation" -Value $InstallDir
Set-ItemProperty -Path $uninstallKey -Name "DisplayIcon" -Value $targetPath
Set-ItemProperty -Path $uninstallKey -Name "UninstallString" -Value ("powershell.exe -ExecutionPolicy Bypass -File `"" + (Join-Path $scriptDir "Uninstall-FlaggedEmailsWidget.ps1") + "`"")

if ($LaunchAfterInstall) {
    Start-Process -FilePath $targetPath -WorkingDirectory $InstallDir
}

Write-Host "Install complete"
Write-Host "Installed to: $InstallDir"
