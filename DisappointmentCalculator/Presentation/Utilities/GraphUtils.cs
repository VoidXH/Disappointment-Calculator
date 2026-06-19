using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;

using DisappointmentCalculator.Data;

namespace DisappointmentCalculator.Presentation.Utilities;

/// <summary>
/// Graph display extensions specific to this application.
/// </summary>
public static class GraphUtils {
    /// <summary>
    /// Set up a <paramref name="graph"/> for grouping data by month.
    /// </summary>
    public static void SetupTimeUnits(this CartesianChart graph, DataAggregator data) => graph.XAxes = [
        new WhiteAxis {
            Labels = data.Labels
        }
    ];

    /// <summary>
    /// Set up a <paramref name="graph"/> for cost display in US dollars.
    /// </summary>
    public static void SetupForCost(this CartesianChart graph, DataAggregator data) {
        graph.SetupTimeUnits(data);
        graph.YAxes = [
            new WhiteAxis {
                Name = "Cost [$]",
                MinLimit = 0,
                Labeler = value => value.ToString("N0")
            }
        ];
    }

    /// <summary>
    /// Set up a <paramref name="graph"/> for duration display in mm:ss format.
    /// </summary>
    public static void SetupForDuration(this CartesianChart graph, DataAggregator data) {
        SetupTimeUnits(graph, data);
        graph.YAxes = [
            new WhiteAxis {
                Name = "Duration",
                MinLimit = 0,
                Labeler = value => TimeSpan.FromMilliseconds(value).ToString(@"hh\:mm\:ss")
            }
        ];
    }

    /// <summary>
    /// Set up a <paramref name="graph"/> for token count display by month.
    /// </summary>
    public static void SetupForTokens(this CartesianChart graph, DataAggregator data) {
        SetupTimeUnits(graph, data);
        graph.YAxes = [
            new WhiteAxis {
                Name = "Tokens",
                MinLimit = 0,
                Labeler = value => value.ToString("N0")
            }
        ];
    }

    /// <summary>
    /// Paints text white on an axis.
    /// </summary>
    class WhiteAxis : Axis {
        /// <summary>
        /// Paints text white on an axis.
        /// </summary>
        public WhiteAxis() {
            NamePaint = new SolidColorPaint(SKColors.White);
            LabelsPaint = new SolidColorPaint(SKColors.White);
        }
    }
}
