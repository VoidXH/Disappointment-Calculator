using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;

using DisappointmentCalculator.Data;
using DisappointmentCalculator.Presentation.Utilities;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// Displays total token use per model in each group.
/// </summary>
public partial class TokensPerModelGraph : CartesianChart {
    /// <summary>
    /// Source of the displayed metrics.
    /// </summary>
    public DataAggregator DataSource {
        set => Setup(this, value);
    }

    /// <summary>
    /// Setup any <paramref name="graph"/> to visualize token use per model based on the <paramref name="data"/>.
    /// </summary>
    public static void Setup(CartesianChart graph, DataAggregator data) {
        graph.Series = [.. data.TokenUseByModel.Select(model => new ColumnSeries<long> {
            Name = model.Key,
            Values = model.Value
        })];
        graph.SetupForTokens(data);
    }
}
