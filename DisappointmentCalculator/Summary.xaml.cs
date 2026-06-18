using System.Globalization;
using System.Text;

using DisappointmentCalculator.Data;

namespace DisappointmentCalculator;

/// <summary>
/// Data visualization for token use metrics.
/// </summary>
public partial class SummaryPage : ContentPage {
    /// <summary>
    /// Data visualization for token use metrics.
    /// </summary>
    public SummaryPage() {
        InitializeComponent();
        DataAggregator data = ((AppShell)Shell.Current).Data;
        if (data.Sessions == null || data.Sessions.Count == 0) {
            return;
        }

        graph1.DataSource = data;
        graph2.DataSource = data;
        graph3.DataSource = data;
        graph4.DataSource = data;

        CalculateMoneyBurn(data);
        CalculateTokenmaxxing(data);
    }

    /// <summary>
    /// Calculate total cost of tokens.
    /// </summary>
    void CalculateMoneyBurn(DataAggregator data) {
        StringBuilder perModelTotal = new();
        decimal globalTotal = 0;
        foreach (KeyValuePair<string, decimal[]> model in data.CostByModel.OrderBy(x => x.Key)) {
            decimal cost = model.Value.Sum();
            perModelTotal.Append(model.Key).Append(": ").AppendLine(cost.ToString("$0.00"));
            globalTotal += cost;
        }
        grandTotal.Text = globalTotal.ToString("$0.00");
        modelTotal.Text = perModelTotal.ToString();
    }

    /// <summary>
    /// Fill display values regarding token usage.
    /// </summary>
    void CalculateTokenmaxxing(DataAggregator data) {
        CultureInfo culturewithSpaces = CultureInfo.GetCultureInfo("fr-FR");
        StringBuilder perModelTotal = new();
        decimal globalTotal = 0;
        foreach (KeyValuePair<string, long[]> model in data.TokenUseByModel.OrderBy(x => x.Key)) {
            long tokens = model.Value.Sum();
            perModelTotal.Append(model.Key).Append(": ").AppendLine(tokens.ToString("N0", culturewithSpaces));
            globalTotal += tokens;
        }
        tokenmaxxing.Text = globalTotal.ToString("N0", culturewithSpaces);
        tokenTotal.Text = perModelTotal.ToString();
    }
}
