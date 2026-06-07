param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "publish"
$distRoot = Join-Path $PSScriptRoot "dist"
$stagingDir = Join-Path $distRoot "FlaggedEmailsWidgetInstaller"
$appStagingDir = Join-Path $stagingDir "app"
$zipPath = Join-Path $distRoot "FlaggedEmailsWidgetInstaller-$Runtime.zip"

if (Test-Path $distRoot) {
    Remove-Item $distRoot -Recurse -Force
}

Write-Host "Publishing app..."
dotnet publish (Join-Path $repoRoot "NotificationWidget.csproj") -c $Configuration -r $Runtime --self-contained true /p:PublishSingleFile=true -o $publishDir

Write-Host "Preparing installer payload..."
New-Item -ItemType Directory -Path $appStagingDir -Force | Out-Null

Copy-Item -Path (Join-Path $publishDir "*") -Destination $appStagingDir -Recurse -Force
Copy-Item -Path (Join-Path $PSScriptRoot "Install-FlaggedEmailsWidget.ps1") -Destination $stagingDir -Force
Copy-Item -Path (Join-Path $PSScriptRoot "Uninstall-FlaggedEmailsWidget.ps1") -Destination $stagingDir -Force
Copy-Item -Path (Join-Path $PSScriptRoot "install.cmd") -Destination $stagingDir -Force
Copy-Item -Path (Join-Path $PSScriptRoot "uninstall.cmd") -Destination $stagingDir -Force
Copy-Item -Path (Join-Path $PSScriptRoot "README.md") -Destination $stagingDir -Force

Write-Host "Creating zip..."
Compress-Archive -Path (Join-Path $stagingDir "*") -DestinationPath $zipPath -Force

Write-Host "Installer package created: $zipPath"
