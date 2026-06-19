using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;

using DisappointmentCalculator.Data;
using DisappointmentCalculator.Presentation.Utilities;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// Displays token use split by category in each group.
/// </summary>
public partial class TokenBreakdownGraph : CartesianChart {
    /// <summary>
    /// Source of the displayed metrics.
    /// </summary>
    public DataAggregator DataSource {
        set => Setup(this, value);
    }

    /// <summary>
    /// Setup any <paramref name="graph"/> to visualize token breakdown based on the <paramref name="data"/>.
    /// </summary>
    public static void Setup(CartesianChart graph, DataAggregator data) {
        graph.Series = [
            new ColumnSeries<long> {
                Name = "Cached input tokens",
                Values = [.. data.Aggregated.Select(x => x.Value.CacheReadTokens)]
            },
            new ColumnSeries<long> {
                Name = "Non-cached input tokens",
                Values = [.. data.Aggregated.Select(x => x.Value.NonCachedInputTokens)]
            },
            new ColumnSeries<long> {
                Name = "Output tokens",
                Values = [.. data.Aggregated.Select(x => x.Value.OutputTokens)]
            },
            new ColumnSeries<long> {
                Name = "Reasoning tokens",
                Values = [.. data.Aggregated.Select(x => x.Value.ReasoningTokens)]
            }
        ];
        graph.SetupForTokens(data);
    }
}
