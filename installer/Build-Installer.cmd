@echo off
setlocal

set "REPO_ROOT=%~dp0.."
set "PUBLISH_DIR=%REPO_ROOT%\publish"
set "DIST_ROOT=%~dp0dist"
set "STAGING_DIR=%DIST_ROOT%\FlaggedEmailsWidgetInstaller"
set "APP_STAGING_DIR=%STAGING_DIR%\app"
set "ZIP_PATH=%DIST_ROOT%\FlaggedEmailsWidgetInstaller-win-x64.zip"

if exist "%DIST_ROOT%" rmdir /s /q "%DIST_ROOT%"

echo Publishing app...
dotnet publish "%REPO_ROOT%\NotificationWidget.csproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "%PUBLISH_DIR%"
if errorlevel 1 (
  echo Publish failed.
  exit /b 1
)

echo Preparing installer payload...
mkdir "%APP_STAGING_DIR%"
xcopy "%PUBLISH_DIR%\*" "%APP_STAGING_DIR%\" /E /I /Y >nul
copy /Y "%~dp0install.cmd" "%STAGING_DIR%\" >nul
copy /Y "%~dp0uninstall.cmd" "%STAGING_DIR%\" >nul
copy /Y "%~dp0README.md" "%STAGING_DIR%\" >nul

echo Creating zip...
powershell.exe -NoProfile -Command "Compress-Archive -Path '%STAGING_DIR%\*' -DestinationPath '%ZIP_PATH%' -Force"
if errorlevel 1 (
  echo Zip creation failed.
  exit /b 1
)

echo Installer package created:
echo %ZIP_PATH%
