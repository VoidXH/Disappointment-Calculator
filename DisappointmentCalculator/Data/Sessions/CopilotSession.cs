using System.Text.Json;

using DisappointmentCalculator.Data.Sessions.BaseClasses;

namespace DisappointmentCalculator.Data.Sessions;

/// <summary>
/// Represents a single session of GitHub Copilot.
/// </summary>
public class CopilotSession : Session {
    /// <summary>
    /// Parses a Copilot events.jsonl file.
    /// </summary>
    /// <param name="filePath">Path to the events.jsonl file to parse</param>
    public CopilotSession(string filePath) {
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines) {
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            using JsonDocument doc = JsonDocument.Parse(line);
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("type", out JsonElement typeProp) || typeProp.GetString() != "session.shutdown") {
                continue;
            }

            JsonElement data = root.GetProperty("data");
            if (data.TryGetProperty("sessionStartTime", out JsonElement sessionStartTime)) {
                SessionStartTime = sessionStartTime.GetInt64();
            }
            if (data.TryGetProperty("totalApiDurationMs", out JsonElement totalApiDurationMs)) {
                TotalApiDurationMs = totalApiDurationMs.GetInt64();
            }
            if (data.TryGetProperty("totalNanoAiu", out JsonElement totalNanoAiu)) {
                TotalNanoAIU = totalNanoAiu.GetInt64();
            }

            Dictionary<string, TokenUsage> result = [];
            if (data.TryGetProperty("modelMetrics", out JsonElement modelMetrics)) {
                foreach (JsonProperty model in modelMetrics.EnumerateObject()) {
                    TokenUsage metrics = new();
                    JsonElement usage = model.Value.GetProperty("usage");
                    if (usage.TryGetProperty("inputTokens", out JsonElement inputTokens)) {
                        metrics.InputTokens = inputTokens.GetInt32();
                    }
                    if (usage.TryGetProperty("outputTokens", out JsonElement outputTokens)) {
                        metrics.OutputTokens = outputTokens.GetInt32();
                    }
                    if (usage.TryGetProperty("cacheReadTokens", out JsonElement cacheReadTokens)) {
                        metrics.CacheReadTokens = cacheReadTokens.GetInt32();
                    }
                    if (usage.TryGetProperty("cacheWriteTokens", out JsonElement cacheWriteTokens)) {
                        metrics.CacheWriteTokens = cacheWriteTokens.GetInt32();
                    }
                    if (usage.TryGetProperty("reasoningTokens", out JsonElement reasoningTokens)) {
                        metrics.ReasoningTokens = reasoningTokens.GetInt32();
                    }
                    result[model.Name] = metrics;
                }
            }
            ModelMetrics = result;
            break;
        }
    }
}
