using System;
using System.IO;
using System.Windows;

namespace NotificationWidget
{
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon? _trayIcon;
        private FileStream? _singleInstanceLock;

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            if (!TryAcquireSingleInstanceLock())
            {
                Shutdown();
                return;
            }

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
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Topmost = true;
            mainWindow.Activate();
            mainWindow.Focus();
            mainWindow.Topmost = false;
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
            _singleInstanceLock?.Dispose();
            base.OnExit(e);
        }

        private bool TryAcquireSingleInstanceLock()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var lockDirectory = Path.Combine(appData, "NotificationWidget");
                Directory.CreateDirectory(lockDirectory);

                var lockPath = Path.Combine(lockDirectory, "instance.lock");
                _singleInstanceLock = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
