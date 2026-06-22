using System.IO;
using System.Text.Json;

using DisappointmentCalculator.Data.Sessions.BaseClasses;
using DisappointmentCalculator.Data.Sessions.VSCode;
using DisappointmentCalculator.Utilities;

namespace DisappointmentCalculator.Data.Sessions;

/// <summary>
/// Represents a single session of VS Code with AI interaction data.
/// Parses session files from %APPDATA%\Code\User\workspaceStorage.
/// </summary>
public class VSCodeSession : Session {
    /// <summary>
    /// Parses a VS Code session file.
    /// </summary>
    /// <param name="filePath">Path to the session file to parse</param>
    public VSCodeSession(string filePath) {
        LastWriteTime = File.GetLastWriteTimeUtc(filePath);

        string currentModel = string.Empty;
        Dictionary<string, TokenUsage> modelMetrics = [];
        foreach (string line in File.ReadLines(filePath)) {
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            using JsonDocument doc = JsonDocument.Parse(line);
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("kind", out JsonElement kindProp) || !kindProp.TryGetInt32(out int kind) ||
                !root.TryGetProperty("v", out JsonElement v)) {
                continue;
            }

            int inputTokens = 0;
            int outputTokens = 0;
            int reasoningTokens = 0;

            if (kind == (int)MessageKinds.System) {
                if (v.TryGetProperty("creationDate", out JsonElement creationDate)) {
                    SessionStartTime = creationDate.GetInt64();
                }
                if (v.TryGetNestedProperty(out JsonElement modelId, "inputState", "selectedModel", "metadata", "id")) {
                    currentModel = modelId.GetString();
                }
            } else if (kind == (int)MessageKinds.VObject) {
                if (root.LastArrayElementEquals("k", "result")) {
                    if (currentModel.Length != 0 && v.TryGetNestedProperty(out JsonElement totalElapsedProp, "timings", "totalElapsed") &&
                        totalElapsedProp.TryGetInt64(out long apiDurationMs)) {
                        TotalApiDurationMs += apiDurationMs;
                    }

                    if (v.TryGetProperty("metadata", out JsonElement metadata)) {
                        inputTokens = ParseTokenCountFrom(metadata, "renderedUserMessage");
                        outputTokens = ParseTokenCountFrom(metadata, "toolCallRounds");
                    }
                }
            } else if (kind == (int)MessageKinds.VArray) {
                foreach (JsonElement element in v.EnumerateArray()) {
                    if (element.TryGetProperty("kind", out JsonElement entryKind) && entryKind.GetString() == "thinking") {
                        reasoningTokens += element.TryGetProperty("value", out JsonElement thinking) ? thinking.GetString().Length / charactersPerToken : 0;
                    }
                    if (element.TryGetNestedProperty(out JsonElement text, "message", "text")) {
                        inputTokens += text.GetString().Length / charactersPerToken;
                    }
                }
            }

            if ((inputTokens != 0 || outputTokens != 0 || reasoningTokens != 0) && currentModel.Length != 0) {
                if (modelMetrics.TryGetValue(currentModel, out TokenUsage usage)) {
                    usage.InputTokens += inputTokens;
                    usage.OutputTokens += outputTokens;
                    usage.ReasoningTokens += reasoningTokens;
                } else {
                    modelMetrics[currentModel] = new TokenUsage {
                        InputTokens = inputTokens,
                        OutputTokens = outputTokens,
                        ReasoningTokens = reasoningTokens
                    };
                }
            }
        }

        ModelMetrics = modelMetrics;
    }

    /// <summary>
    /// Gets all session file paths from the VS Code workspaceStorage chatSessions directories.
    /// Searches %APPDATA%\Code\User\workspaceStorage\*\chatSessions\*.jsonl.
    /// </summary>
    /// <returns>An enumerable of (sessionId, eventsFile) tuples where sessionId is the filename without dashes.</returns>
    public static IEnumerable<(Guid sessionId, string eventsFile)> GetSessionFiles(DateTime lastUpdate) {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string baseDir = Path.Combine(appData, "Code", "User", "workspaceStorage");

        if (!Directory.Exists(baseDir)) {
            return [];
        }

        List<(Guid sessionId, string eventsFile)> result = [];
        string[] workspaceFolders = Directory.GetDirectories(baseDir);

        foreach (string workspaceFolder in workspaceFolders) {
            string chatSessionsDir = Path.Combine(workspaceFolder, "chatSessions");
            if (!Directory.Exists(chatSessionsDir)) {
                continue;
            }

            string[] jsonlFiles = Directory.GetFiles(chatSessionsDir, "*.jsonl");
            foreach (string file in jsonlFiles) {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (File.GetLastWriteTime(fileName) >= lastUpdate) {
                    string noDashes = fileName.Replace("-", "");
                    string formatted = $"{noDashes[..8]}-{noDashes[8..12]}-{noDashes[12..16]}-{noDashes[16..20]}-{noDashes[20..]}";
                    Guid sessionId = Guid.Parse(formatted);
                    result.Add((sessionId, file));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Deletes locally stored VS Code chat session cache files.
    /// </summary>
    public static void WipeCache() {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string baseDir = Path.Combine(appData, "Code", "User", "workspaceStorage");

        if (!Directory.Exists(baseDir)) {
            return;
        }

        foreach (string workspaceFolder in Directory.GetDirectories(baseDir)) {
            string chatSessionsDir = Path.Combine(workspaceFolder, "chatSessions");
            CacheCleanup.DeleteEntriesBeforeToday(chatSessionsDir);
        }
    }

    /// <summary>
    /// Parse estimated token counts from an entire <see cref="JsonElement"/>.
    /// </summary>
    static int ParseTokenCountFrom(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement child) ? child.GetRawText().Length / charactersPerToken : 0;

    /// <summary>
    /// A ratio of characters to tokens as 4 is an educated guess since we don't have true token metrics.
    /// </summary>
    const int charactersPerToken = 4;
}
