using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DisappointmentCalculator.Data.Sessions.BaseClasses;

/// <summary>
/// Parses a GitHub Copilot session's events.jsonl file for token usage metrics.
/// </summary>
public class Session {
    /// <summary>
    /// Dictionary of model name to its request metrics.
    /// </summary>
    public IReadOnlyDictionary<string, TokenUsage> ModelMetrics { get; init; }

    /// <summary>
    /// Session start time as a Unix timestamp in milliseconds.
    /// </summary>
    public long SessionStartTime { get; init; }

    /// <summary>
    /// When the file was last written (UTC).
    /// </summary>
    public DateTime LastWriteTime { get; init; }

    /// <summary>
    /// AI credits used, billionths.
    /// </summary>
    public long TotalNanoAIU { get; init; }

    /// <summary>
    /// API duration in milliseconds.
    /// </summary>
    public long TotalApiDurationMs { get; init; }

    /// <summary>
    /// Private property initialization constructor.
    /// </summary>
    [JsonConstructor]
    protected Session() { }

    /// <summary>
    /// Combine the input <paramref name="sessions"/> to a total.
    /// </summary>
    public static Session Merge(params Session[] sessions) {
        if (sessions == null || sessions.Length == 0) {
            return new();
        }

        long lowestStartTime = sessions.Min(x => x.SessionStartTime);
        long totalNanoAiu = 0;
        long totalApiDurationMs = 0;
        Dictionary<string, TokenUsage> mergedMetrics = [];

        foreach (Session session in sessions) {
            totalNanoAiu += session.TotalNanoAIU;
            totalApiDurationMs += session.TotalApiDurationMs;

            foreach (KeyValuePair<string, TokenUsage> kvp in session.ModelMetrics) {
                string modelName = kvp.Key;
                TokenUsage source = kvp.Value;

                if (mergedMetrics.TryGetValue(modelName, out TokenUsage existing)) {
                    existing.InputTokens += source.InputTokens;
                    existing.OutputTokens += source.OutputTokens;
                    existing.CacheReadTokens += source.CacheReadTokens;
                    existing.CacheWriteTokens += source.CacheWriteTokens;
                    existing.ReasoningTokens += source.ReasoningTokens;
                } else {
                    mergedMetrics[modelName] = new TokenUsage {
                        InputTokens = source.InputTokens,
                        OutputTokens = source.OutputTokens,
                        CacheReadTokens = source.CacheReadTokens,
                        CacheWriteTokens = source.CacheWriteTokens,
                        ReasoningTokens = source.ReasoningTokens
                    };
                }
            }
        }

        Session merged = new() {
            SessionStartTime = lowestStartTime,
            TotalNanoAIU = totalNanoAiu,
            TotalApiDurationMs = totalApiDurationMs,
            ModelMetrics = mergedMetrics
        };
        return merged;
    }

    /// <summary>
    /// Combine the input <paramref name="sessions"/> into a single entry which contains all models' <see cref="TokenUsage"/>.
    /// </summary>
    public static TokenUsage MergeModels(params Session[] sessions) {
        if (sessions == null || sessions.Length == 0) {
            return new();
        }

        TokenUsage totalMetrics = new();
        foreach (Session session in sessions) {
            foreach (TokenUsage source in session.ModelMetrics.Values) {
                totalMetrics.InputTokens += source.InputTokens;
                totalMetrics.OutputTokens += source.OutputTokens;
                totalMetrics.CacheReadTokens += source.CacheReadTokens;
                totalMetrics.CacheWriteTokens += source.CacheWriteTokens;
                totalMetrics.ReasoningTokens += source.ReasoningTokens;
            }
        }
        return totalMetrics;
    }

    /// <summary>
    /// Get how many US dollars are spent on running this <paramref name="model"/> for this session.
    /// </summary>
    public decimal GetCostForModel(ModelPricing model) {
        if (ModelMetrics.Count == 1 && TotalNanoAIU != 0) {
            return TotalNanoAIU / 10_000_000m; // 100 AIC = $1
        }
        return ModelMetrics.TryGetValue(model.LoggedName, out TokenUsage usage) ?
            new ModelSession(usage, model).TotalCost :
            0m;
    }

    /// <summary>
    /// Get the total cost in US dollars for this session across all models.
    /// </summary>
    public decimal GetTotalCost() {
        if (ModelMetrics.Count == 1 && TotalNanoAIU != 0) {
            return TotalNanoAIU / 10_000_000m; // 100 AIC = $1
        }

        decimal total = 0m;
        foreach (KeyValuePair<string, TokenUsage> kvp in ModelMetrics) {
            string modelName = kvp.Key;
            TokenUsage usage = kvp.Value;
            ModelPricing pricing = ModelDatabase.GetPricing(modelName);
            total += new ModelSession(usage, pricing).TotalCost;
        }
        return total;
    }

    /// <summary>
    /// Saves the current session metrics to a JSON file.
    /// </summary>
    /// <param name="filePath">The target file path to save the JSON data</param>
    public void Save(string filePath) {
        string json = JsonSerializer.Serialize<Session>(this, jsonOptions);
        string directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory)) {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Cached serialization settings
    /// </summary>
    static readonly JsonSerializerOptions jsonOptions = new() {
        WriteIndented = false
    };
}
