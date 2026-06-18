using LiveChartsCore.SkiaSharpView.Maui;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using DisappointmentCalculator.Data;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// A chart with a drop-down to switch between graph types.
/// </summary>
public partial class SelectableGraph : Grid {
    /// <summary>
    /// Gets or sets the currently displayed graph type.
    /// </summary>
    public GraphType SelectedGraphType {
        get => selectedGraphType;
        set {
            if (selectedGraphType != value) {
                selectedGraphType = value;
                picker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
                picker.SelectedIndex = (int)selectedGraphType;
                picker.SelectedIndexChanged -= OnPickerSelectedIndexChanged;
                ApplyDataSource();
            }
        }
    }
    GraphType selectedGraphType = GraphType.Tokens;

    /// <summary>
    /// Sets the data source for the currently selected graph.
    /// </summary>
    public DataAggregator DataSource {
        set {
            dataSource = value;
            ApplyDataSource();
        }
    }
    DataAggregator dataSource;

    /// <summary>
    /// Graph instance allocated for displaying the data.
    /// </summary>
    readonly CartesianChart graph = new();

    /// <summary>
    /// Dropdown that chooses between graph types.
    /// </summary>
    readonly Picker picker = new();

    /// <summary>
    /// A chart with a drop-down to switch between graph types.
    /// </summary>
    public SelectableGraph() {
        string[] graphTypeNames = [.. Enum.GetValues<GraphType>().Select(GetDisplayName)];

        picker.ItemsSource = graphTypeNames;
        picker.SelectedIndex = (int)selectedGraphType;
        picker.SelectedIndexChanged += OnPickerSelectedIndexChanged;

        graph.BackgroundColor = Colors.Transparent;

        Grid.SetRow(picker, 0);
        Grid.SetRow(graph, 1);
        Grid.SetColumnSpan(graph, 2);

        RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        RowDefinitions.Add(new RowDefinition(GridLength.Star));
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        Children.Add(picker);
        Children.Add(graph);

        ApplyDataSource();
    }

    /// <summary>
    /// Gets the display name for an enum value from its [Display] attribute.
    /// </summary>
    static string GetDisplayName(GraphType type) {
        MemberInfo member = type.GetType().GetMember(type.ToString())[0];
        DisplayAttribute attribute = (DisplayAttribute)member.GetCustomAttributes(typeof(DisplayAttribute), false)[0];
        return attribute.Name;
    }

    /// <summary>
    /// Display the received data on the <see cref="graph"/>.
    /// </summary>
    void ApplyDataSource() {
        if (dataSource == null) {
            return;
        }

        DataAggregator data = dataSource;
        switch (selectedGraphType) {
            case GraphType.Tokens:
                TokenGraph.Setup(graph, data);
                break;
            case GraphType.TokensPerModel:
                TokensPerModelGraph.Setup(graph, data);
                break;
            case GraphType.TokenBreakdown:
                TokenBreakdownGraph.Setup(graph, data);
                break;
            case GraphType.Cost:
                CostGraph.Setup(graph, data);
                break;
            case GraphType.CostPerModel:
                CostPerModelGraph.Setup(graph, data);
                break;
            case GraphType.TimeSpentInApi:
                TimeSpentInApiGraph.Setup(graph, data);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The user has changed the type of <see cref="graph"/> displayed.
    /// </summary>
    void OnPickerSelectedIndexChanged(object _, EventArgs e) {
        if (picker.SelectedItem is string displayName) {
            foreach (GraphType type in Enum.GetValues<GraphType>()) {
                if (GetDisplayName(type) == displayName) {
                    selectedGraphType = type;
                    ApplyDataSource();
                    break;
                }
            }
        }
    }
}
