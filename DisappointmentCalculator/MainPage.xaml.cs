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
            LoadButton.IsEnabled = false;
            StatusLabel.Content = "Loading session data...";
            Progress.Visibility = Visibility.Visible;
            Progress.Value = 0;

            try {
                await LoadData(Progress);
            } catch (SessionFileInUseException ex) {
                StatusLabel.Content = "Warning: a session file is still in use. Exit all AI IDEs and CLIs, then try again.";
                Progress.Visibility = Visibility.Collapsed;
                LoadButton.IsEnabled = true;

                MessageBox.Show(
                    $"A session file is still in use:\n\n{ex.FilePath}\n\nExit all AI IDEs and CLIs, then try loading data again.",
                    "Session file in use",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            StatusLabel.Content = "Data loaded successfully.";
            Progress.Value = 100;
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Data = mainWindow.Data; // Data is already set
            mainWindow.ShowPage(new SummaryPage());
        }
    }
}
