using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace NotificationWidget
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _refreshTimer = new();
        private const double SettingsPanelExpandedWidth = 188;
        private bool _isRefreshing;
        private bool _refreshQueued;
        private bool _suppressScanLimitSelectionChange;
        private bool _firstRenderLogged;
        private bool _isSettingsPanelOpen;
        private int _maxEmailsToCheck = 1000;
        private static readonly BlockingCollection<WorkItem> OutlookWorkQueue = new();
        private static readonly Thread OutlookWorkerThread = CreateOutlookWorkerThread();

        private sealed class WorkItem
        {
            public required TaskCompletionSource<InboxSummary> CompletionSource { get; init; }
            public required int MaxEmailsToCheck { get; init; }
        }

        private sealed class WidgetSettings
        {
            public int MaxEmailsToCheck { get; set; } = 1000;
        }

        public MainWindow()
        {
            InitializeComponent();
            SettingsPanel.Visibility = Visibility.Collapsed;
            LoadSettings();

            ContentRendered += MainWindow_ContentRendered;

            _refreshTimer.Interval = TimeSpan.FromMinutes(1);
            _refreshTimer.Tick += async (s, e) => await RefreshFlaggedItemsAsync();
        }

        private void MainWindow_ContentRendered(object? sender, EventArgs e)
        {
            if (_firstRenderLogged) return;
            _firstRenderLogged = true;
            StartupPerfLog.Write("Main window first render completed");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PositionWindowForStartup();

            // At sign-in, Explorer/screen metrics can still be settling; retry once shortly after load.
            Dispatcher.BeginInvoke(new Action(PositionWindowForStartup), DispatcherPriority.ApplicationIdle);

            Activate();
            StatusText.Text = "Loading flagged emails...";
            _ = RefreshFlaggedItemsAsync();
            _refreshTimer.Start();
        }

        private void PositionWindowForStartup()
        {
            var primaryWorkArea = Forms.Screen.PrimaryScreen?.WorkingArea;
            var virtualLeft = SystemParameters.VirtualScreenLeft;
            var virtualTop = SystemParameters.VirtualScreenTop;
            var virtualRight = virtualLeft + SystemParameters.VirtualScreenWidth;
            var virtualBottom = virtualTop + SystemParameters.VirtualScreenHeight;

            var width = ActualWidth > 0 ? ActualWidth : Width;
            var height = ActualHeight > 0 ? ActualHeight : Height;

            var targetRight = primaryWorkArea?.Right ?? (int)virtualRight;
            var targetTop = primaryWorkArea?.Top ?? (int)virtualTop;

            var left = targetRight - width - 16;
            var top = targetTop + 16;

            var maxLeft = Math.Max(virtualLeft, virtualRight - width);
            var maxTop = Math.Max(virtualTop, virtualBottom - height);

            Left = Math.Min(Math.Max(left, virtualLeft), maxLeft);
            Top = Math.Min(Math.Max(top, virtualTop), maxTop);
            StartupPerfLog.Write($"Window positioned at ({Left:0},{Top:0}) size=({width:0}x{height:0})");
        }

        private async Task RefreshFlaggedItemsAsync()
        {
            if (_isRefreshing)
            {
                // If a refresh is already running (for example during Outlook COM calls),
                // queue exactly one follow-up refresh so UI actions still take effect quickly.
                _refreshQueued = true;
                return;
            }

            _isRefreshing = true;
            var refreshStopwatch = Stopwatch.StartNew();

            try
            {
                var summary = await GetInboxSummaryAsync(_maxEmailsToCheck);
                var flaggedItems = summary.FlaggedEmails;
                int unreadCount = summary.UnreadCount;
                int count = flaggedItems.Count;
                FlaggedItemsList.ItemsSource = count > 0 ? flaggedItems : null;
                TitleText.Text = $"🚩 ({count}) Flagged | 📬 {unreadCount} Unread";
                var elapsedSeconds = Math.Round(refreshStopwatch.Elapsed.TotalSeconds, 1);
                StatusText.Text = count > 0
                    ? $"Updated {DateTime.Now:t} ({elapsedSeconds}s)."
                    : $"No flagged emails. Updated {DateTime.Now:t} ({elapsedSeconds}s).";
                StartupPerfLog.Write($"Refresh complete in {refreshStopwatch.ElapsedMilliseconds}ms (flagged={count}, unread={unreadCount}, scanLimit={_maxEmailsToCheck})");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                StartupPerfLog.Write($"Refresh failed in {refreshStopwatch.ElapsedMilliseconds}ms ({ex.GetType().Name})");
            }
            finally
            {
                _isRefreshing = false;
                if (_refreshQueued)
                {
                    _refreshQueued = false;
                    _ = RefreshFlaggedItemsAsync();
                }
            }
        }

        private static Task<InboxSummary> GetInboxSummaryAsync(int maxEmailsToCheck)
        {
            var completionSource = new TaskCompletionSource<InboxSummary>();
            OutlookWorkQueue.Add(new WorkItem
            {
                CompletionSource = completionSource,
                MaxEmailsToCheck = maxEmailsToCheck
            });
            return completionSource.Task;
        }

        private static Thread CreateOutlookWorkerThread()
        {
            var thread = new Thread(() =>
            {
                foreach (var workItem in OutlookWorkQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        workItem.CompletionSource.SetResult(OutlookService.GetInboxSummary(workItem.MaxEmailsToCheck));
                    }
                    catch (Exception ex)
                    {
                        workItem.CompletionSource.SetException(ex);
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) DragMove();
        }

        private void FlaggedItemsList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FlaggedItemsList.SelectedItem is FlaggedEmail email && !string.IsNullOrEmpty(email.EntryID))
                OutlookService.OpenEmail(email.EntryID, email.StoreID);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSettingsPanel(!_isSettingsPanelOpen);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Refreshing...";
            await RefreshFlaggedItemsAsync();
        }

        private void Window_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_isSettingsPanelOpen) return;

            var source = e.OriginalSource as DependencyObject;
            if (source == null) return;
            if (IsDescendantOf(source, SettingsPanel)) return;
            if (IsDescendantOf(source, SettingsButton)) return;

            ToggleSettingsPanel(false);
        }

        private static bool IsDescendantOf(DependencyObject child, DependencyObject ancestor)
        {
            var current = child;
            while (current != null)
            {
                if (ReferenceEquals(current, ancestor)) return true;
                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private void ToggleSettingsPanel(bool open)
        {
            _isSettingsPanelOpen = open;
            if (open)
            {
                SettingsPanel.Visibility = Visibility.Visible;
            }

            var targetWidth = open ? SettingsPanelExpandedWidth : 0;
            var animation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            if (!open)
            {
                animation.Completed += (_, _) => SettingsPanel.Visibility = Visibility.Collapsed;
            }

            SettingsPanel.BeginAnimation(FrameworkElement.WidthProperty, animation);
        }

        private async void ScanLimitCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_suppressScanLimitSelectionChange) return;
            if (ScanLimitCombo.SelectedItem is not System.Windows.Controls.ComboBoxItem item) return;
            if (!int.TryParse(item.Tag?.ToString(), out var newLimit)) return;
            if (newLimit == _maxEmailsToCheck) return;

            _maxEmailsToCheck = newLimit;
            SaveSettings();
            StatusText.Text = $"Scan set to {_maxEmailsToCheck}. Refreshing...";
            StartupPerfLog.Write($"Scan limit changed to {_maxEmailsToCheck}");
            ToggleSettingsPanel(false);
            await RefreshFlaggedItemsAsync();
        }

        private static string GetSettingsPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "NotificationWidget", "widget-settings.json");
        }

        private void LoadSettings()
        {
            try
            {
                var settingsPath = GetSettingsPath();
                if (!File.Exists(settingsPath))
                {
                    SelectScanLimitInUi(_maxEmailsToCheck);
                    return;
                }

                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<WidgetSettings>(json);
                if (settings == null) return;

                _maxEmailsToCheck = Math.Clamp(settings.MaxEmailsToCheck, 500, 3000);
                SelectScanLimitInUi(_maxEmailsToCheck);
                StartupPerfLog.Write($"Loaded settings scanLimit={_maxEmailsToCheck}");
            }
            catch (Exception ex)
            {
                StartupPerfLog.Write($"LoadSettings failed ({ex.GetType().Name})");
                SelectScanLimitInUi(_maxEmailsToCheck);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settingsPath = GetSettingsPath();
                var settingsDirectory = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(settingsDirectory))
                {
                    Directory.CreateDirectory(settingsDirectory);
                }

                var json = JsonSerializer.Serialize(new WidgetSettings { MaxEmailsToCheck = _maxEmailsToCheck });
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                StartupPerfLog.Write($"SaveSettings failed ({ex.GetType().Name})");
            }
        }

        private void SelectScanLimitInUi(int scanLimit)
        {
            // Update ComboBox selection without triggering a refresh while we are restoring state.
            _suppressScanLimitSelectionChange = true;
            try
            {
                for (int i = 0; i < ScanLimitCombo.Items.Count; i++)
                {
                    if (ScanLimitCombo.Items[i] is ComboBoxItem item &&
                        int.TryParse(item.Tag?.ToString(), out var tagValue) &&
                        tagValue == scanLimit)
                    {
                        ScanLimitCombo.SelectedIndex = i;
                        return;
                    }
                }

                ScanLimitCombo.SelectedIndex = 1;
            }
            finally
            {
                _suppressScanLimitSelectionChange = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
    }
}

