namespace DisappointmentCalculator.Data;

/// <summary>
/// Breakdown of token usage for a single model with a single time span.
/// </summary>
public class TokenUsage {
    /// <summary>
    /// Number of tokens sent to the model, including system prompt, tools, prompts, and files.
    /// </summary>
    public long InputTokens { get; set; }

    /// <summary>
    /// Number of tokens received as displayed answers.
    /// </summary>
    public long OutputTokens { get; set; }

    /// <summary>
    /// Number of tokens reused from cache at a lower price.
    /// </summary>
    public long CacheReadTokens { get; set; }

    /// <summary>
    /// Number of tokens added to cache.
    /// </summary>
    public long CacheWriteTokens { get; set; }

    /// <summary>
    /// Number of tokens spent in chain of thought operations.
    /// </summary>
    public long ReasoningTokens { get; set; }

    /// <summary>
    /// Total tokens paid with full input costs.
    /// </summary>
    public long NonCachedInputTokens => InputTokens - CacheReadTokens;

    /// <summary>
    /// Total tokens wasted on this prompt.
    /// </summary>
    public long TotalTokens => InputTokens + OutputTokens + ReasoningTokens;
}
