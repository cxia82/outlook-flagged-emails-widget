using System;
using System.Windows;
using System.Windows.Threading;

namespace NotificationWidget
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _refreshTimer = new();

        public MainWindow()
        {
            InitializeComponent();
            _refreshTimer.Interval = TimeSpan.FromMinutes(1);
            _refreshTimer.Tick += (s, e) => RefreshFlaggedItems();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 16;
            Top = workArea.Top + 16;
            Activate();
            RefreshFlaggedItems();
            _refreshTimer.Start();
        }

        private void RefreshFlaggedItems()
        {
            try
            {
                var flaggedItems = OutlookService.GetFlaggedEmails();
                int count = flaggedItems.Count;
                FlaggedItemsList.ItemsSource = count > 0 ? flaggedItems : null;
                TitleText.Text = count > 0 ? $"🚩 ({count}) Flagged Emails" : "🚩 Flagged Emails";
                StatusText.Text = count > 0 ? $"Updated {DateTime.Now:t}." : $"No flagged emails. Checked {DateTime.Now:t}.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
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

