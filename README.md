# NotificationWidget

A simple Windows desktop notification widget that appears at the top-right corner of the screen.

## Files

- `NotificationWidget.csproj` - WPF project file targeting .NET 8 Windows.
- `App.xaml` / `App.xaml.cs` - application entry point.
- `MainWindow.xaml` / `MainWindow.xaml.cs` - top-right notification widget UI and positioning.

## Run

1. Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
2. Open this folder in Visual Studio or VS Code.
3. Build and run the project.

## Behavior

- The window is borderless and transparent.
- It is positioned in the top-right corner of the primary work area.
- The app fetches flagged email items from the Outlook Inbox and lists them in the widget.
- The widget refreshes flagged Outlook emails automatically every minute.
- The notification automatically closes after 10 minutes.
