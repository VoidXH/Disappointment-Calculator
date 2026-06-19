using System.Windows;
using System.Windows.Controls;
using DisappointmentCalculator.Data;

namespace DisappointmentCalculator {
    public partial class MainWindow : Window {
        public DataAggregator Data { get; set; }

        public MainWindow() {
            InitializeComponent();
            ShowPage(new MainPage());
        }

        internal void ShowPage(Control page) {
            ContentControl pageContainer = (ContentControl)FindName("PageContainer");
            pageContainer?.Content = page;
        }
    }
}
