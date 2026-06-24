using System.IO;

using DisappointmentCalculator.Data.Sessions;
using DisappointmentCalculator.Data.Sessions.BaseClasses;
using DisappointmentCalculator.Enums;
using DisappointmentCalculator.Utilities;

namespace DisappointmentCalculator.Data;

/// <summary>
/// Finds supported AI sessions.
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
            DateTime key = session.LastWriteTime.Date;
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
    /// Parses all supported local AI sessions into a dictionary keyed by session GUID.
    /// </summary>
    /// <param name="progress">Optional progress reporter reporting ratio of session folders processed (0-1)</param>
    /// <returns>A dictionary mapping each session's Guid to its Session object. Returns an empty dictionary if the directory does not exist.</returns>
    public static async Task<SessionCollection> ParseSessions(IProgress<double> progress = null) {
        // Load cache to skip folders before cached date
        SessionCollection sessions = [];
        DateTime lastUpdate = SessionCache.LastCacheUpdate;
        IEnumerable<(Guid, string)> cachedSessions = SessionCache.GetSessionFiles();
        IEnumerable<(Guid, string)> antigravitySessions = AntigravitySession.GetSessionFiles(lastUpdate);
        IEnumerable<(Guid, string)> copilotSessions = CopilotSession.GetSessionFiles(lastUpdate);
        IEnumerable<(Guid, string)> codexSessions = CodexSession.GetSessionFiles(lastUpdate);
        IEnumerable<(Guid, string)> vsCodeSessions = VSCodeSession.GetSessionFiles(lastUpdate);
        int total = cachedSessions.Count() + antigravitySessions.Count() + copilotSessions.Count() + codexSessions.Count() + vsCodeSessions.Count();
        int processed = 0;

        void ParseSet(IEnumerable<(Guid, string)> unparsedSessions, Func<string, Session> constructor) {
            foreach ((Guid sessionId, string eventsFile) in unparsedSessions) {
                processed++;

                Session session = null;
                try {
                    session = constructor(eventsFile);
                } catch (IOException e) when (SessionFileInUseException.IsFileInUse(e)) {
                    throw new SessionFileInUseException(eventsFile, e);
                } catch {
                    // Skip sessions that fail to parse
                }
                if (session != null) {
                    sessions[sessionId] = session;
                }

                ReportProgress(progress, processed, total);
            }
        }

        ParseSet(antigravitySessions, x => new AntigravitySession(x));
        ParseSet(copilotSessions, x => new CopilotSession(x));
        ParseSet(codexSessions, x => new CodexSession(x));
        ParseSet(vsCodeSessions, x => new VSCodeSession(x));
        SessionCache.UpdateCache(sessions); // Only cache what's not already cached
        ParseSet(cachedSessions, x => new CachedSession(x));
        return sessions;
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
    /// Deletes local cache/session storage for every supported session format.
    /// </summary>
    public static void WipeCache() {
        AntigravitySession.WipeCache();
        CopilotSession.WipeCache();
        CodexSession.WipeCache();
        VSCodeSession.WipeCache();
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
