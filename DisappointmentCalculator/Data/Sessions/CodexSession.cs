using System.Globalization;
using System.IO;
using System.Text.Json;

using DisappointmentCalculator.Data.Sessions.BaseClasses;

namespace DisappointmentCalculator.Data.Sessions;

/// <summary>
/// Represents a single session of OpenAI Codex CLI.
/// </summary>
public class CodexSession : Session {
    /// <summary>
    /// Parses a Codex rollout JSONL file.
    /// </summary>
    /// <param name="filePath">Path to the rollout JSONL file to parse</param>
    public CodexSession(string filePath) {
        LastWriteTime = File.GetLastWriteTimeUtc(filePath);

        string currentModel = string.Empty;
        Dictionary<string, TokenUsage> modelMetrics = [];

        foreach (string line in File.ReadLines(filePath)) {
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            using JsonDocument doc = JsonDocument.Parse(line);
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("type", out JsonElement typeProp) ||
                !root.TryGetProperty("payload", out JsonElement payload)) {
                continue;
            }

            string type = typeProp.GetString();
            if (type == "session_meta") {
                SessionStartTime = ParseSessionStartTime(payload);
            } else if (type == "turn_context") {
                if (payload.TryGetProperty("model", out JsonElement model)) {
                    currentModel = model.GetString() ?? string.Empty;
                }
            } else if (type == "event_msg" &&
                payload.TryGetProperty("type", out JsonElement payloadType) &&
                payloadType.GetString() == "token_count") {
                AddTokenUsage(payload, currentModel, modelMetrics);
            }
        }

        if (modelMetrics.Count == 0) {
            throw new InvalidDataException();
        }

        ModelMetrics = modelMetrics;
    }

    /// <summary>
    /// Gets all session file paths from the Codex sessions directory.
    /// </summary>
    /// <returns>An enumerable of tuples containing the session Guid and the rollout JSONL file path.</returns>
    public static IEnumerable<(Guid sessionId, string eventsFile)> GetSessionFiles(DateTime lastUpdate) {
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string sessionsDir = Path.Combine(homeDir, ".codex", "sessions");

        if (!Directory.Exists(sessionsDir)) {
            return [];
        }

        List<(Guid SessionId, string EventsFile)> result = [];
        foreach (string file in Directory.EnumerateFiles(sessionsDir, "rollout-*.jsonl", SearchOption.AllDirectories)) {
            if (File.GetLastWriteTimeUtc(file) < lastUpdate) {
                continue;
            }

            try {
                if (TryGetSessionId(file, out Guid sessionId)) {
                    result.Add((sessionId, file));
                }
            } catch (IOException e) when (SessionFileInUseException.IsFileInUse(e)) {
                throw new SessionFileInUseException(file, e);
            }
        }

        return result;
    }

    /// <summary>
    /// Reads the session start timestamp from a Codex session metadata payload.
    /// </summary>
    static long ParseSessionStartTime(JsonElement payload) {
        if (payload.TryGetProperty("timestamp", out JsonElement timestamp) &&
            DateTimeOffset.TryParse(timestamp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset startTime)) {
            return startTime.ToUnixTimeMilliseconds();
        }
        return 0;
    }

    /// <summary>
    /// Adds the token usage from one Codex token_count event to the current model total.
    /// </summary>
    static void AddTokenUsage(JsonElement payload, string currentModel, Dictionary<string, TokenUsage> modelMetrics) {
        if (!payload.TryGetProperty("info", out JsonElement info) ||
            !info.TryGetProperty("last_token_usage", out JsonElement usage)) {
            return;
        }

        string modelName = currentModel.Length == 0 ? "unknown" : currentModel;
        if (!modelMetrics.TryGetValue(modelName, out TokenUsage metrics)) {
            metrics = new();
            modelMetrics[modelName] = metrics;
        }

        metrics.InputTokens += GetInt64(usage, "input_tokens");
        metrics.OutputTokens += GetInt64(usage, "output_tokens");
        metrics.CacheReadTokens += GetInt64(usage, "cached_input_tokens");
        metrics.ReasoningTokens += GetInt64(usage, "reasoning_output_tokens");
    }

    /// <summary>
    /// Reads a numeric property from a JSON object, returning zero when it is absent or not numeric.
    /// </summary>
    static long GetInt64(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) && property.TryGetInt64(out long value) ? value : 0;

    /// <summary>
    /// Reads the Codex session id from the first session_meta record in a rollout file.
    /// </summary>
    static bool TryGetSessionId(string filePath, out Guid sessionId) {
        foreach (string line in File.ReadLines(filePath)) {
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            using JsonDocument doc = JsonDocument.Parse(line);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("type", out JsonElement type) &&
                type.GetString() == "session_meta" &&
                root.TryGetProperty("payload", out JsonElement payload) &&
                payload.TryGetProperty("id", out JsonElement id)) {
                return Guid.TryParse(id.GetString(), out sessionId);
            }
        }

        sessionId = default;
        return false;
    }
}
