using DisappointmentCalculator.Data;

namespace DisappointmentCalculator {
    public partial class AppShell : Shell {
        public DataAggregator Data { get; set; }

        public AppShell() => InitializeComponent();
    }
}
