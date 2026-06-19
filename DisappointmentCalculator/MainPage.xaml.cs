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

            await LoadData(Progress);

            StatusLabel.Content = "Data loaded successfully.";
            Progress.Value = 100;
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Data = mainWindow.Data; // Data is already set
            mainWindow.ShowPage(new SummaryPage());
        }
    }
}
