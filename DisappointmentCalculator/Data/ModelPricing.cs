namespace DisappointmentCalculator.Data;

/// <summary>
/// Contains information of a single model's pricing in each of the priced categories.
/// </summary>
/// <param name="Name">Full stylized name of the model</param>
/// <param name="LoggedName">How Copilot calls the model</param>
/// <param name="InputPricing">Dollars per million tokens input</param>
/// <param name="OutputPricing">Dollars per million tokens output</param>
/// <param name="CacheReadPricing">Dollars per million tokens of cache hits</param>
/// <param name="CacheWritePricing">Dollars per million tokens saved to cache</param>
public sealed record class ModelPricing(
    string Name,
    string LoggedName,
    decimal InputPricing,
    decimal OutputPricing,
    decimal CacheReadPricing,
    decimal CacheWritePricing
) { }
