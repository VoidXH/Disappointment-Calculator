using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;

using DisappointmentCalculator.Data;
using DisappointmentCalculator.Presentation.Utilities;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// Displays total tokens used in each of the selected time spans.
/// </summary>
public partial class TokenGraph : CartesianChart {
    /// <summary>
    /// Source of the displayed metrics.
    /// </summary>
    public DataAggregator DataSource {
        set => Setup(this, value);
    }

    /// <summary>
    /// Setup any <paramref name="graph"/> to visualize total tokens based on the <paramref name="data"/>.
    /// </summary>
    public static void Setup(CartesianChart graph, DataAggregator data) {
        graph.Series = [
            new ColumnSeries<long> {
                Name = "Total tokens",
                Values = [.. data.Aggregated.Select(x => x.Value.TotalTokens)]
            }
        ];
        graph.SetupForTokens(data);
    }
}
