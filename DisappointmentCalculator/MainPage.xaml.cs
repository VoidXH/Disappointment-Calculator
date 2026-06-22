using System.IO;
using System.Windows;
using System.Windows.Controls;

using DisappointmentCalculator.Data;
using DisappointmentCalculator.Enums;

namespace DisappointmentCalculator {
    /// <summary>
    /// Welcome screen of Disappointment Calculator.
    /// </summary>
    public partial class MainPage : UserControl {
        /// <summary>
        /// Welcome screen of Disappointment Calculator.
        /// </summary>
        public MainPage() {
            InitializeComponent();
            AggregationPicker.Items.Add("per month");
            AggregationPicker.Items.Add("per day");
            AggregationPicker.SelectedIndex = 0;
        }

        /// <summary>
        /// Loads Copilot session data using <see cref="SessionDiscovery"/>.
        /// </summary>
        async Task LoadData(ProgressBar progressBar) {
            IProgress<double> progress = new Progress<double>(x => Application.Current.Dispatcher.Invoke(() => progressBar.Value = x * 100));
            GroupBy groupBy = AggregationPicker.SelectedIndex == 0 ? GroupBy.Monthly : GroupBy.Daily;
            SessionCollection sessions = await SessionDiscovery.ParseGroupedSessions(groupBy, progress);

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Data = new(sessions, groupBy);
        }

        /// <summary>
        /// Load session data.
        /// </summary>
        async void OnLoadClicked(object _, RoutedEventArgs e) {
            loadButton.IsEnabled = false;
            status.Content = "Loading session data...";
            progress.Visibility = Visibility.Visible;
            progress.Value = 0;

            try {
                await LoadData(progress);
            } catch (SessionFileInUseException ex) {
                status.Content = "Warning: a session file is still in use. Exit all AI IDEs and CLIs, then try again.";
                progress.Visibility = Visibility.Collapsed;
                loadButton.IsEnabled = true;

                MessageBox.Show(
                    $"A session file is still in use:\n\n{ex.FilePath}\n\nExit all AI IDEs and CLIs, then try loading data again.",
                    "Session file in use",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            status.Content = "Data loaded successfully.";
            progress.Value = 100;
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Data = mainWindow.Data; // Data is already set
            mainWindow.ShowPage(new SummaryPage());
        }

        /// <summary>
        /// Wipes all supported local session caches after user confirmation.
        /// </summary>
        void OnWipeCacheClicked(object _, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show(
                "Do you really want to clear the session caches from this computer? This cannot be undone.",
                "Clear session caches?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );

            if (result != MessageBoxResult.Yes) {
                return;
            }

            wipeCache.IsEnabled = false;
            loadButton.IsEnabled = false;
            status.Content = "Clearing session caches...";

            try {
                SessionDiscovery.WipeCache();
                status.Content = "Session caches cleared.";
            } catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                status.Content = "Could not clear all session caches. Close AI tools and try again.";
                MessageBox.Show(
                    $"Could not clear all session caches:\n\n{ex.Message}",
                    "Clear session caches failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            } finally {
                wipeCache.IsEnabled = true;
                loadButton.IsEnabled = true;
            }
        }
    }
}
