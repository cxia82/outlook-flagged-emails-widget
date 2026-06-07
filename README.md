# Outlook Flagged Emails Widget

**Version 1.0**

A lightweight WPF desktop widget for Windows that displays your flagged Outlook emails in a floating window. It connects directly to your running Outlook application — no credentials, no Azure app registration required.

![Widget screenshot](https://github.com/cxia82/outlook-flagged-emails-widget/raw/master/docs/screenshot.png)

---

## Features

- 🚩 **Flagged email list** — scans your 500 most recent inbox items and shows all actively flagged emails
- 👤 **Sender + subject** — displays both sender name and email subject for each flagged item
- ✂️ **Two-line subject wrap** — long subjects wrap to two lines with `...` ellipsis if truncated
- 🔢 **Flag count in title** — window title shows `(N) Flagged Emails` at a glance
- 🖱️ **Click to open** — double-click any email to open it directly in Outlook
- 🔄 **Auto-refresh** — refreshes every minute automatically
- 📐 **Resizable & draggable** — resize by dragging any edge/corner; drag the title bar to reposition
- 🔕 **System tray** — lives in the notification area (system tray), not the taskbar; click ✕ to hide, right-click tray icon to show/hide or exit
- 🚀 **Auto-start** — registered to launch automatically on Windows login

---

## Requirements

- Windows 10/11
- Microsoft Outlook desktop app (must be installed and running)

---

## Installation

1. Download the latest release from the [Releases](https://github.com/cxia82/outlook-flagged-emails-widget/releases) page
2. Extract `FlaggedEmailsWidgetInstaller-win-x64.zip`
3. Run `install.cmd`
4. Launch the app from the desktop shortcut `Flagged Emails`

For installer details and custom packaging steps, see [installer/README.md](installer/README.md).

---

## Build from Source

```bash
git clone https://github.com/cxia82/outlook-flagged-emails-widget.git
cd outlook-flagged-emails-widget
dotnet publish -c Release -r win-x64 --self-contained false -o publish
```

Then run `publish\NotificationWidget.exe`.

---

## How It Works

The widget uses **COM late-binding** (`Type.GetTypeFromProgID("Outlook.Application")`) to connect to your already-running Outlook desktop app. It reads the default inbox, sorts items by received time (newest first), scans up to 500 items, and filters for `FlagStatus == 2` (actively flagged).

No OAuth tokens, no Exchange credentials, and no Microsoft Graph API calls are needed.

---

## Project Structure

| File | Description |
|------|-------------|
| `App.xaml` / `App.xaml.cs` | App entry point, system tray icon setup |
| `MainWindow.xaml` / `MainWindow.xaml.cs` | Widget UI, positioning, refresh timer |
| `OutlookService.cs` | COM connection to Outlook, email fetching |
| `TwoLineEllipsisTextBlock.cs` | Custom WPF control for 2-line text with ellipsis |
| `app.ico` | Custom app icon (envelope + red flag) |
| `app.manifest` | App manifest (`asInvoker` — no UAC elevation) |
