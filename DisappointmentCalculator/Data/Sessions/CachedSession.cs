using System.IO;
using System.Text.Json;

using DisappointmentCalculator.Data.Sessions.BaseClasses;
using DisappointmentCalculator.Utilities;

namespace DisappointmentCalculator.Data.Sessions;

/// <summary>
/// A <see cref="Session"/> that initializes its properties by loading from a JSON file.
/// </summary>
public sealed class CachedSession : Session {
    /// <summary>
    /// Loads a cached session from the specified JSON file path.
    /// </summary>
    /// <param name="filePath">The path to the JSON file</param>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is malformed or missing properties.</exception>
    public CachedSession(string filePath) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException("Cached session file not found.", filePath);
        }

        string json = File.ReadAllText(filePath);
        Session loadedData = JsonSerializer.Deserialize<Session>(json) ?? throw new JsonException($"Failed to deserialize session data from {filePath}");
        ModelMetrics = loadedData.ModelMetrics ?? new Dictionary<string, TokenUsage>();
        SessionStartTime = loadedData.SessionStartTime;
        LastWriteTime = loadedData.LastWriteTime == default ? GetCacheDate(filePath) : loadedData.LastWriteTime;
        TotalNanoAIU = loadedData.TotalNanoAIU;
        TotalApiDurationMs = loadedData.TotalApiDurationMs;
    }

    /// <summary>
    /// Reads the cache date from cache filenames generated from GuidUtils date keys.
    /// </summary>
    static DateTime GetCacheDate(string filePath) =>
        Guid.TryParse(Path.GetFileNameWithoutExtension(filePath), out Guid cacheKey) ? cacheKey.ToDate() : default;
}
