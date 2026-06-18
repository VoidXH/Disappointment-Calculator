namespace DisappointmentCalculator.Data;

/// <summary>
/// Breakdown of pricing for a single session based on token usage and model pricing.
/// </summary>
public class ModelSession {
    /// <summary>
    /// Cost of input tokens.
    /// </summary>
    public decimal InputCost { get; }

    /// <summary>
    /// Cost of output tokens.
    /// </summary>
    public decimal OutputCost { get; }

    /// <summary>
    /// Cost of cache read tokens.
    /// </summary>
    public decimal CacheReadCost { get; }

    /// <summary>
    /// Cost of cache write tokens.
    /// </summary>
    public decimal CacheWriteCost { get; }

    /// <summary>
    /// Cost of reasoning tokens.
    /// </summary>
    public decimal ReasoningCost { get; }

    /// <summary>
    /// Total cost across all categories.
    /// </summary>
    public decimal TotalCost { get; }

    /// <summary>
    /// Creates a ModelUsage from the given token metrics and model pricing.
    /// </summary>
    /// <param name="metrics">Token usage metrics for the session</param>
    /// <param name="pricing">Model pricing configuration</param>
    public ModelSession(TokenUsage metrics, ModelPricing pricing) {
        const decimal million = 1_000_000m;
        InputCost      = metrics.InputTokens      * pricing.InputPricing      / million;
        OutputCost     = metrics.OutputTokens     * pricing.OutputPricing     / million;
        CacheReadCost  = metrics.CacheReadTokens  * pricing.CacheReadPricing  / million;
        CacheWriteCost = metrics.CacheWriteTokens * pricing.CacheWritePricing / million;
        ReasoningCost  = metrics.ReasoningTokens  * pricing.OutputPricing     / million;
        TotalCost = InputCost + OutputCost + CacheReadCost + CacheWriteCost + ReasoningCost;
    }
}
