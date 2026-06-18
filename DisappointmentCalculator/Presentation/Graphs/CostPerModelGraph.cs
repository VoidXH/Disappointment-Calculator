using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;

using DisappointmentCalculator.Data;
using DisappointmentCalculator.Presentation.Utilities;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// Displays total cost per model in each group.
/// </summary>
public partial class CostPerModelGraph : CartesianChart {
    /// <summary>
    /// Source of the displayed metrics.
    /// </summary>
    public DataAggregator DataSource {
        set => Setup(this, value);
    }

    /// <summary>
    /// Setup any <paramref name="graph"/> to visualize total cost per model based on the <paramref name="data"/>.
    /// </summary>
    public static void Setup(CartesianChart graph, DataAggregator data) {
        graph.Series = [.. data.CostByModel.Select(model => new ColumnSeries<decimal> {
            Name = model.Key,
            Values = model.Value
        })];
        graph.SetupForCost(data);
    }
}
