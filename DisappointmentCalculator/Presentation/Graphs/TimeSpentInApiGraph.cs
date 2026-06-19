using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;

using DisappointmentCalculator.Data;
using DisappointmentCalculator.Presentation.Utilities;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// Displays total time spent in API in each of the selected time spans.
/// </summary>
public partial class TimeSpentInApiGraph : CartesianChart {
    /// <summary>
    /// Source of the displayed metrics.
    /// </summary>
    public DataAggregator DataSource {
        set => Setup(this, value);
    }

    /// <summary>
    /// Setup any <paramref name="graph"/> to visualize total time spent in API based on the <paramref name="data"/>.
    /// </summary>
    public static void Setup(CartesianChart graph, DataAggregator data) {
        graph.Series = [
            new ColumnSeries<long> {
                Name = "Time in API",
                Values = [.. data.TotalApiDurationMs]
            }
        ];
        graph.SetupForDuration(data);
    }
}
