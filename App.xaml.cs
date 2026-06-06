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

            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico")),
                Text = "Flagged Emails",
                Visible = true
            };

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Show / Hide", null, (s, ev) => ToggleWindow());
            menu.Items.Add("Exit", null, (s, ev) => { _trayIcon.Visible = false; Shutdown(); });
            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (s, ev) => ToggleWindow();

            MainWindow?.Show();
        }

        private void ToggleWindow()
        {
            if (MainWindow == null) return;
            if (MainWindow.IsVisible)
                MainWindow.Hide();
            else
            {
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
