using LiveChartsCore.SkiaSharpView.WPF;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using DisappointmentCalculator.Data;

using WPFGridUnitType = System.Windows.GridUnitType;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// A chart with a drop-down to switch between graph types.
/// </summary>
public class SelectableGraph : Grid {
    /// <summary>
    /// Gets or sets the currently displayed graph type.
    /// </summary>
    public GraphType SelectedGraphType {
        get => selectedGraphType;
        set {
            if (selectedGraphType != value) {
                selectedGraphType = value;
                picker.SelectionChanged -= OnPickerSelectionChanged;
                picker.SelectedIndex = (int)selectedGraphType;
                picker.SelectionChanged += OnPickerSelectionChanged;
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
    readonly ComboBox picker = new();

    /// <summary>
    /// A chart with a drop-down to switch between graph types.
    /// </summary>
    public SelectableGraph() {
        string[] graphTypeNames = [.. Enum.GetValues<GraphType>().Select(GetDisplayName)];

        picker.ItemsSource = graphTypeNames;
        picker.SelectedIndex = (int)selectedGraphType;
        picker.SelectionChanged += OnPickerSelectionChanged;

        graph.Background = Brushes.Transparent;

        picker.HorizontalAlignment = HorizontalAlignment.Stretch;
        graph.HorizontalAlignment = HorizontalAlignment.Stretch;
        graph.VerticalAlignment = VerticalAlignment.Stretch;

        SetRow(picker, 0);
        SetRow(graph, 1);
        SetColumnSpan(graph, 2);

        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, WPFGridUnitType.Auto) });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, WPFGridUnitType.Star) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, WPFGridUnitType.Star) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, WPFGridUnitType.Star) });

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
    void OnPickerSelectionChanged(object _, SelectionChangedEventArgs e) {
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
