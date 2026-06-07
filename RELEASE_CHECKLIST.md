# NotificationWidget Release Checklist

Use this checklist for every production release.

## 1) Pre-release checks

- Confirm branch is correct and up to date:
  - `git status --short`
  - `git pull --ff-only`
- Ensure no unintended files are staged:
  - Exclude local-only artifacts like workspace files and installer dist folders unless intentionally included.
- Tag a rollback point before major changes:
  - `git tag -a <rollback-tag> -m "Rollback point"`
  - `git push origin <rollback-tag>`
  - `git push mck <rollback-tag>`

## 2) Build and runtime validation

- Stop running app instances first to avoid publish lock:
  - `taskkill /IM NotificationWidget.exe /F`
- Build:
  - `dotnet build .\NotificationWidget.csproj`
- Publish:
  - `dotnet publish .\NotificationWidget.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\publish`
- Smoke test launch:
  - Start app from desktop shortcut and confirm process is running.
- Crash check:
  - Confirm no recent `.NET Runtime` / `Application Error` events for NotificationWidget.

## 3) Functional verification

- Verify title shows both counts:
  - Flagged count + unread count format is correct.
- Verify flagged list behavior:
  - Items render correctly and double-click opens mail.
- Verify tray behavior:
  - Show/Hide and Exit work.
- Verify startup behavior:
  - HKCU Run entry exists and executable path is quoted.

## 4) Installer packaging

- Rebuild installer package from latest code:
  - `cmd /c .\installer\Build-Installer.cmd`
- Confirm output exists:
  - `.\installer\dist\FlaggedEmailsWidgetInstaller-win-x64.zip`
- Installer script checks:
  - Install adds shortcuts, Run key, uninstall entry.
  - Uninstall removes shortcuts (including redirected desktop path), Run key, install folder.

## 5) Documentation updates

- Update release notes and README for any new feature/behavior changes.
- Confirm version references are consistent:
  - README version
  - Installer DisplayVersion in install script
  - Release tag version

## 6) Git publish

- Commit final changes:
  - `git add <files>`
  - `git commit -m "<release message>"`
- Push to both remotes:
  - `git push origin master`
  - `git push mck master`

## 7) GitHub release

- Create or update release tag (example: `v1.1.1`).
- Upload latest installer asset:
  - `FlaggedEmailsWidgetInstaller-win-x64.zip`
- Verify public links:
  - Release page opens correctly.
  - Direct download URL works.

## 8) Post-release validation

- Download installer from release and perform a clean install test.
- Confirm app auto-starts after sign-in.
- Confirm startup-perf log still rotates and does not grow unbounded.
- Record release summary and any incidents/lessons learned.
