using DisappointmentCalculator.Data;
using DisappointmentCalculator.Enums;

namespace DisappointmentCalculator {
    /// <summary>
    /// Welcome screen of Disappointment Calculator.
    /// </summary>
    public partial class MainPage : ContentPage {
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
            IProgress<double> progress = new Progress<double>(x => progressBar.Dispatcher.Dispatch(() => progressBar.Progress = x));
            GroupBy groupBy = AggregationPicker.SelectedIndex == 0 ? GroupBy.Monthly : GroupBy.Daily;
            SessionCollection sessions = await SessionDiscovery.ParseGroupedSessions(groupBy, progress);

            AppShell currentShell = (AppShell)Shell.Current;
            currentShell.Data = new(sessions, groupBy);
        }

        /// <summary>
        /// Load session data.
        /// </summary>
        async void OnLoadClicked(object _, EventArgs e) {
            LoadButton.IsEnabled = false;
            StatusLabel.Text = "Loading session data...";
            Progress.IsVisible = true;
            Progress.Progress = 0;

            await LoadData(Progress);

            StatusLabel.Text = "Data loaded successfully.";
            Progress.Progress = 1;
            await Shell.Current.GoToAsync("///SummaryPage");
        }
    }
}
