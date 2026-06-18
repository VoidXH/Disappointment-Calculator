namespace DisappointmentCalculator.Data;

/// <summary>
/// Pricing information of known models.
/// </summary>
public static class ModelDatabase {
    /// <summary>
    /// Get pricing information for a model by the name which copilot is using.
    /// </summary>
    public static ModelPricing GetPricing(string loggedName) => catalog.TryGetValue(loggedName, out ModelPricing result) ?
        result :
        new(loggedName, loggedName, 0, 0, 0, 0);

    /// <summary>
    /// Pricing information of known models.
    /// </summary>
    static readonly Dictionary<string, ModelPricing> catalog = new() {
        { "claude-fable-5", new("Anthropic Claude Fable 5", "claude-fable-5", 10m, 50m, 10m, 12.5m) },
        { "claude-haiku-4.5", new("Anthropic Claude Haiku 4.5", "claude-haiku-4.5", 1m, 5m, 0.1m, 1.25m) },
        { "claude-opus-4.5", new("Anthropic Claude Opus 4.5", "claude-opus-4.5", 5m, 25m, 0.5m, 6.25m) },
        { "claude-opus-4.7", new("Anthropic Claude Opus 4.7", "claude-opus-4.7", 5m, 25m, 0.5m, 6.25m) },
        { "claude-opus-4.8", new("Anthropic Claude Opus 4.8", "claude-opus-4.8", 5m, 25m, 0.5m, 6.25m) },
        { "claude-sonnet-4.5", new("Anthropic Claude Sonnet 4.5", "claude-sonnet-4.5", 3m, 15m, 0.3m, 3.75m) },
        { "claude-sonnet-4.6", new("Anthropic Claude Sonnet 4.6", "claude-sonnet-4.6", 3m, 15m, 0.3m, 3.75m) },
        { "gemini-2.5-pro", new("Google Gemini 2.5 Pro", "gemini-2.5-pro", 2m, 12m, 0.2m, 2.5m) },
        { "gemini-3-flash", new("Google Gemini 3 Flash", "gemini-3-flash", 0.075m, 0.3m, 075m, 0.0938m) },
        { "gemini-3.1-pro", new("Google Gemini 3.1 Pro", "gemini-3.1-pro", 2m, 12m, 0.2m, 2.5m) },
        { "gemini-3.5-flash", new("Google Gemini 3.5 Flash", "gemini-3.5-flash", 1.5m, 9m, 0.15m, 1.875m) },
        { "gpt-4", new("OpenAI GPT-4", "gpt-4", 2.5m, 15m, 1.25m, 2.5m) },
        { "gpt-5-mini", new("OpenAI GPT-5 Mini", "gpt-5-mini", 0.25m, 2m, 0.02m, 0.3125m) },
        { "gpt-5.3-codex", new("OpenAI GPT-5.3 Codex", "gpt-5.3-codex", 1.75m, 14m, 0.175m, 2.1875m) },
        { "gpt-5.4", new("OpenAI GPT-5.4 (Normal Context)", "gpt-5.4", 2.5m, 15m, 0.25m, 3.125m) },
        { "gpt-5.4-long", new("OpenAI GPT-5.4 (Long Context >272k)", "gpt-5.4-long", 5m, 22.5m, 0.5m, 6.25m) },
        { "gpt-5.4-mini", new("OpenAI GPT-5.4 Mini", "gpt-5.4-mini", 0.75m, 4.5m, 0.075m, 0.9375m) },
        { "gpt-5.4-nano", new("OpenAI GPT-5.4 Nano", "gpt-5.4-nano", 0.2m, 1.25m, 0.02m, 0.25m) },
        { "gpt-5.5", new("OpenAI GPT-5.5 (Normal Context)", "gpt-5.5", 5m, 30m, 0.5m, 6.25m) },
        { "gpt-5.5-long", new("OpenAI GPT-5.5 (Long Context >272k)", "gpt-5.5-long", 10m, 45m, 10m, 12.5m) },
        { "VoidAI", new("VoidAI", "VoidAI", 0.0069444m, 0.069444m, 0, 0) },
    };
}
