using System.ComponentModel.DataAnnotations;

namespace DisappointmentCalculator.Presentation.Graphs;

/// <summary>
/// Types of graphs that can be selected in a SelectableGraph.
/// </summary>
public enum GraphType {
    /// <summary>
    /// Total tokens used, grouped by time span.
    /// </summary>
    [Display(Name = "Tokens")]
    Tokens,

    /// <summary>
    /// Tokens per model, grouped by time span.
    /// </summary>
    [Display(Name = "Tokens per Model")]
    TokensPerModel,

    /// <summary>
    /// Types of tokens used, grouped by time span.
    /// </summary>
    [Display(Name = "Token Breakdown")]
    TokenBreakdown,

    /// <summary>
    /// Total money spent on API requests, grouped by time span.
    /// </summary>
    [Display(Name = "Cost")]
    Cost,

    /// <summary>
    /// Money spent on API requests, grouped by time span.
    /// </summary>
    [Display(Name = "Cost per Model")]
    CostPerModel,

    /// <summary>
    /// Time spent processing prompts, grouped by time span.
    /// </summary>
    [Display(Name = "Time Spent in API")]
    TimeSpentInApi,
}
