using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NotificationWidget
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _refreshTimer = new();
        private bool _isRefreshing;
        private bool _firstRenderLogged;

        public MainWindow()
        {
            InitializeComponent();

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
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 16;
            Top = workArea.Top + 16;
            Activate();
            StatusText.Text = "Loading flagged emails...";
            _ = RefreshFlaggedItemsAsync();
            _refreshTimer.Start();
        }

        private async Task RefreshFlaggedItemsAsync()
        {
            if (_isRefreshing) return;
            _isRefreshing = true;
            var refreshStopwatch = Stopwatch.StartNew();

            try
            {
                var summary = await GetInboxSummaryAsync();
                var flaggedItems = summary.FlaggedEmails;
                int unreadCount = summary.UnreadCount;
                int count = flaggedItems.Count;
                FlaggedItemsList.ItemsSource = count > 0 ? flaggedItems : null;
                TitleText.Text = $"🚩 ({count}) Flagged | 📬 {unreadCount} Unread";
                StatusText.Text = count > 0 ? $"Updated {DateTime.Now:t}." : $"No flagged emails. Checked {DateTime.Now:t}.";
                StartupPerfLog.Write($"Refresh complete in {refreshStopwatch.ElapsedMilliseconds}ms (flagged={count}, unread={unreadCount})");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                StartupPerfLog.Write($"Refresh failed in {refreshStopwatch.ElapsedMilliseconds}ms ({ex.GetType().Name})");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private static Task<InboxSummary> GetInboxSummaryAsync()
        {
            var completionSource = new TaskCompletionSource<InboxSummary>();

            var thread = new Thread(() =>
            {
                try
                {
                    completionSource.SetResult(OutlookService.GetInboxSummary());
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return completionSource.Task;
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

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
    }
}

