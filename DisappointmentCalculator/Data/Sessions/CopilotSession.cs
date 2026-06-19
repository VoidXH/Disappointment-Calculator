using System.IO;
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
        LastWriteTime = File.GetLastWriteTimeUtc(filePath);
        foreach (string line in File.ReadLines(filePath)) {
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

        if (ModelMetrics == null) {
            throw new InvalidDataException();
        }
    }

    /// <summary>
    /// Gets all session file paths from the GitHub Copilot session-state directory.
    /// </summary>
    /// <returns>An enumerable of tuples containing the session Guid and the events.jsonl file path.</returns>
    public static IEnumerable<(Guid sessionId, string eventsFile)> GetSessionFiles(DateTime lastUpdate) {
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string sessionStateDir = Path.Combine(homeDir, ".copilot", "session-state");

        if (!Directory.Exists(sessionStateDir)) {
            return [];
        }

        string[] sessionDirectories = Directory.GetDirectories(sessionStateDir);
        List<(Guid SessionId, string EventsFile)> result = [];

        foreach (string sessionDir in sessionDirectories) {
            if (!Guid.TryParse(Path.GetFileName(sessionDir), out Guid sessionId)) {
                continue;
            }

            string eventsFile = Path.Combine(sessionDir, "events.jsonl");
            if (File.Exists(eventsFile) && File.GetLastWriteTime(sessionDir) >= lastUpdate) {
                result.Add((sessionId, eventsFile));
            }
        }

        return result;
    }
}
