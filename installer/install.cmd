@echo off
setlocal

set "SOURCE_DIR=%~dp0app"
set "INSTALL_DIR=%LOCALAPPDATA%\Programs\FlaggedEmailsWidget"
set "TARGET_EXE=%INSTALL_DIR%\NotificationWidget.exe"
set "ICON_PATH=%INSTALL_DIR%\app.ico"

if not exist "%SOURCE_DIR%\NotificationWidget.exe" (
  echo Installer payload missing: %SOURCE_DIR%\NotificationWidget.exe
  pause
  exit /b 1
)

if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
xcopy "%SOURCE_DIR%\*" "%INSTALL_DIR%\" /E /I /Y >nul
if errorlevel 1 (
  echo Failed to copy application files.
  pause
  exit /b 1
)

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "FlaggedEmailsWidget" /t REG_SZ /d "\"%TARGET_EXE%\"" /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run" /v "FlaggedEmailsWidget" /t REG_BINARY /d 020000000000000000000000 /f >nul 2>&1

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" /v "DisplayName" /t REG_SZ /d "Flagged Emails Widget" /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" /v "Publisher" /t REG_SZ /d "outlook-flagged-emails-widget" /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" /v "DisplayVersion" /t REG_SZ /d "1.1" /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" /v "InstallLocation" /t REG_SZ /d "%INSTALL_DIR%" /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" /v "DisplayIcon" /t REG_SZ /d "%TARGET_EXE%" /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" /v "UninstallString" /t REG_SZ /d "\"%~dp0uninstall.cmd\"" /f >nul

powershell.exe -NoProfile -Command "$w=New-Object -ComObject WScript.Shell; $desktop=[Environment]::GetFolderPath('Desktop'); $programs=[Environment]::GetFolderPath('Programs'); $smFolder=Join-Path $programs 'Flagged Emails Widget'; New-Item -ItemType Directory -Path $smFolder -Force | Out-Null; $desktopLnk=Join-Path $desktop 'Flagged Emails.lnk'; $startLnk=Join-Path $smFolder 'Flagged Emails.lnk'; $s1=$w.CreateShortcut($desktopLnk); $s1.TargetPath='%TARGET_EXE%'; $s1.WorkingDirectory='%INSTALL_DIR%'; $s1.Description='Flagged Emails Widget'; if (Test-Path '%ICON_PATH%') { $s1.IconLocation='%ICON_PATH%,0' }; $s1.Save(); $s2=$w.CreateShortcut($startLnk); $s2.TargetPath='%TARGET_EXE%'; $s2.WorkingDirectory='%INSTALL_DIR%'; $s2.Description='Flagged Emails Widget'; if (Test-Path '%ICON_PATH%') { $s2.IconLocation='%ICON_PATH%,0' }; $s2.Save();"

start "" "%TARGET_EXE%"

echo Installation succeeded.
echo Installed to: %INSTALL_DIR%
pause
