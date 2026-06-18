using System.Text.Json;

using DisappointmentCalculator.Data.Sessions;
using DisappointmentCalculator.Data.Sessions.BaseClasses;
using DisappointmentCalculator.Enums;
using DisappointmentCalculator.Utilities;

namespace DisappointmentCalculator.Data;

/// <summary>
/// Finds GitHub Copilot's sessions.
/// </summary>
public static class SessionDiscovery {
    /// <summary>
    /// Merges models of each time group (already created with <see cref="ParseGroupedSessions(IProgress{double})"/>), or of any previous merging,
    /// to a combined total token usage of all models per merging unit.
    /// </summary>
    public static Dictionary<Guid, TokenUsage> Collapse(this SessionCollection sessions) {
        Dictionary<Guid, TokenUsage> merged = [];
        foreach (KeyValuePair<Guid, Session> group in sessions) {
            merged[group.Key] = Session.MergeModels(group.Value);
        }
        return merged;
    }

    /// <summary>
    /// Group a set of <paramref name="sessions"/> by an aggregation time unit.
    /// </summary>
    public static SessionCollection GroupSessions(SessionCollection sessions, GroupBy groupBy) {
        Dictionary<DateTime, List<Session>> groups = [];
        foreach (KeyValuePair<Guid, Session> kvp in sessions) {
            Session session = kvp.Value;
            if (session.SessionStartTime <= 0) {
                continue;
            }

            DateTime key = DateTimeOffset.FromUnixTimeMilliseconds(session.SessionStartTime).Date;
            if (groupBy == GroupBy.Monthly) {
                key = new DateTime(key.Year, key.Month, 1);
            }

            if (!groups.TryGetValue(key, out List<Session> group)) {
                group = [];
                groups[key] = group;
            }
            group?.Add(session);
        }

        SessionCollection groupedSessions = [];
        foreach (KeyValuePair<DateTime, List<Session>> group in groups) {
            DateTime date = group.Key;
            Session merged = Session.Merge([.. group.Value]);
            groupedSessions[GuidUtils.ToGuid(date.Year, date.Month, date.Day, groupBy)] = merged;
        }

        return groupedSessions;
    }

    /// <summary>
    /// Parses all Copilot sessions from the .copilot/session-state directory into a dictionary keyed by session GUID.
    /// </summary>
    /// <param name="progress">Optional progress reporter reporting ratio of session folders processed (0-1)</param>
    /// <returns>A dictionary mapping each session's Guid to its Session object. Returns an empty dictionary if the directory does not exist.</returns>
    public static async Task<SessionCollection> ParseSessions(IProgress<double> progress = null) {
        SessionCollection sessions = [];
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string sessionStateDir = Path.Combine(homeDir, ".copilot", "session-state");

        if (!Directory.Exists(sessionStateDir)) {
            return sessions;
        }

        // Load cache to skip folders before cached date
        SessionCollection cachedSessions = SessionCache.LoadCache();
        DateTime lastUpdate = SessionCache.LastCacheUpdate;
        long cacheDateMs = lastUpdate == default ? 0 : new DateTimeOffset(lastUpdate).ToUnixTimeMilliseconds();

        string[] sessionDirectories = Directory.GetDirectories(sessionStateDir);
        int total = sessionDirectories.Length;
        int processed = 0;

        foreach (string sessionDir in sessionDirectories) {
            processed++;

            if (!Guid.TryParse(Path.GetFileName(sessionDir), out Guid sessionId)) {
                continue;
            }

            string eventsFile = Path.Combine(sessionDir, "events.jsonl");
            if (File.Exists(eventsFile) && IsSessionNewer(eventsFile, cacheDateMs)) {
                try {
                    sessions[sessionId] = new CopilotSession(eventsFile);
                } catch {
                    // Skip sessions that fail to parse
                }
            }

            ReportProgress(progress, processed, total);
            await Task.Yield();
        }

        // Merge cached sessions with newly discovered ones
        foreach (KeyValuePair<Guid, Session> kvp in sessions) {
            cachedSessions[kvp.Key] = kvp.Value;
        }

        // Update cache with newly discovered sessions
        SessionCache.UpdateCache(sessions);
        return cachedSessions;
    }

    /// <summary>
    /// Checks if a session's start time is newer than the cached cutoff.
    /// </summary>
    /// <param name="eventsFile">Path to the events.jsonl file</param>
    /// <param name="cacheDateMs">Unix timestamp in milliseconds for the cache cutoff</param>
    /// <returns>True if the session is newer than the cache cutoff, or false if it can be skipped</returns>
    static bool IsSessionNewer(string eventsFile, long cacheDateMs) {
        if (cacheDateMs == 0) {
            return true; // No cache, process everything
        }

        string[] lines = File.ReadAllLines(eventsFile);
        foreach (string line in lines) {
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            using JsonDocument doc = JsonDocument.Parse(line);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("type", out JsonElement typeProp) && typeProp.GetString() == "session.shutdown") {
                JsonElement data = root.GetProperty("data");
                if (data.TryGetProperty("sessionStartTime", out JsonElement sessionStartTime)) {
                    return sessionStartTime.GetInt64() >= cacheDateMs;
                }
            }
        }
        return true; // If we can't determine the date, parse it to be safe
    }

    /// <summary>
    /// Parses all Copilot sessions and merges sessions from the same time group into a single session.
    /// Each time group is keyed by a Guid derived from the date.
    /// </summary>
    /// <param name="groupBy">Defines what the groups are</param>
    /// <param name="progress">Optional progress reporter reporting ratio of individual session folders processed (0-1)</param>
    /// <returns>A dictionary mapping each timestamp-formatted Guid to the merged Session. Returns an empty dictionary if the directory does not exist.</returns>
    public static async Task<SessionCollection> ParseGroupedSessions(GroupBy groupBy, IProgress<double> progress = null) {
        SessionCollection allSessions = await ParseSessions(progress);
        return GroupSessions(allSessions, groupBy);
    }

    /// <summary>
    /// Reports the current processing progress as a percentage.
    /// </summary>
    /// <param name="progress">The progress reporter to update</param>
    /// <param name="processed">The number of session folders processed so far</param>
    /// <param name="total">The total number of session folders</param>
    static void ReportProgress(IProgress<double> progress, int processed, int total) {
        if (progress is not null && total > 0) {
            progress.Report(processed / (double)total);
        }
    }
}
