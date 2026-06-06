# NotificationWidget

A simple Windows desktop notification widget that lists flagged emails in outlook.

## Files

- `NotificationWidget.csproj` - WPF project file targeting .NET 8 Windows.
- `App.xaml` / `App.xaml.cs` - application entry point.
- `MainWindow.xaml` / `MainWindow.xaml.cs` - top-right notification widget UI and positioning.

## Run

1. Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
2. Open this folder in Visual Studio or VS Code.
3. Build and run the project.

## Behavior

- It is positioned in the top-right corner of the primary work area by default.  You can toggle and move to any position.
- The app fetches flagged email items from the Outlook Inbox out of most recent 500 emails and lists them in the widget.
- The widget refreshes flagged Outlook emails automatically every minute.
