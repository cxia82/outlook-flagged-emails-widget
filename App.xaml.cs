using System;
using System.Windows;

namespace NotificationWidget
{
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon? _trayIcon;

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            StartupPerfLog.Write("App OnStartup begin");

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;

            _trayIcon = new System.Windows.Forms.NotifyIcon();
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (System.IO.File.Exists(iconPath))
                _trayIcon.Icon = new System.Drawing.Icon(iconPath);

            _trayIcon.Text = "Flagged Emails";
            _trayIcon.Visible = true;

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Show / Hide", null, (s, ev) => ToggleWindow());
            menu.Items.Add("Exit", null, (s, ev) => { _trayIcon.Visible = false; Shutdown(); });
            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (s, ev) => ToggleWindow();

            mainWindow.Show();
            mainWindow.Activate();
            StartupPerfLog.Write("Main window shown and activated");
        }

        private void ToggleWindow()
        {
            if (MainWindow == null) return;
            if (MainWindow.IsVisible)
                MainWindow.Hide();
            else
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                    MainWindow.WindowState = WindowState.Normal;
                MainWindow.Show();
                MainWindow.Activate();
            }
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
