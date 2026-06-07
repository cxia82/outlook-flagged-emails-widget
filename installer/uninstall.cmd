@echo off
setlocal

set "INSTALL_DIR=%LOCALAPPDATA%\Programs\FlaggedEmailsWidget"
set "DESKTOP_LNK=%USERPROFILE%\Desktop\Flagged Emails.lnk"
set "START_MENU_DIR=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Flagged Emails Widget"
set "START_MENU_LNK=%START_MENU_DIR%\Flagged Emails.lnk"

taskkill /IM NotificationWidget.exe /F >nul 2>&1

reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "FlaggedEmailsWidget" /f >nul 2>&1
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FlaggedEmailsWidget" /f >nul 2>&1

if exist "%DESKTOP_LNK%" del /f /q "%DESKTOP_LNK%"
if exist "%START_MENU_LNK%" del /f /q "%START_MENU_LNK%"
if exist "%START_MENU_DIR%" rmdir /s /q "%START_MENU_DIR%"
if exist "%INSTALL_DIR%" rmdir /s /q "%INSTALL_DIR%"

echo Uninstall succeeded.
pause
