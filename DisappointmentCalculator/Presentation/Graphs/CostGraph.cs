using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;

using DisappointmentCalculator.Data;
using DisappointmentCalculator.Presentation.Utilities;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// Displays total cost per model in each group.
/// </summary>
public partial class CostGraph : CartesianChart {
    /// <summary>
    /// Source of the displayed metrics.
    /// </summary>
    public DataAggregator DataSource {
        set => Setup(this, value);
    }

    /// <summary>
    /// Setup any <paramref name="graph"/> to visualize total cost per time unit based on the <paramref name="data"/>.
    /// </summary>
    public static void Setup(CartesianChart graph, DataAggregator data) {
        graph.Series = [
            new ColumnSeries<decimal> {
                Name = "Cost",
                Values = data.Cost
            }
        ];
        graph.SetupForCost(data);
    }
}
