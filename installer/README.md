# Flagged Emails Widget Installer

## End user install

1. Download and extract `FlaggedEmailsWidgetInstaller-win-x64.zip`.
2. Open the extracted folder.
3. Run `install.cmd`.
4. If prompted by Windows SmartScreen, choose "More info" then "Run anyway" only if you trust the source.

The installer will:
- Copy app files to `%LOCALAPPDATA%\\Programs\\FlaggedEmailsWidget`
- Create Desktop and Start Menu shortcuts
- Enable auto-start at Windows sign-in for current user
- Add an uninstall entry for current user

## End user uninstall

Run `uninstall.cmd` from the same extracted installer folder.

## Build a new installer package

From repository root:

```cmd
installer\Build-Installer.cmd
```

Output zip:

- `installer\\dist\\FlaggedEmailsWidgetInstaller-win-x64.zip`
