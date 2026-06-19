using DisappointmentCalculator.Data.Sessions;
using DisappointmentCalculator.Data.Sessions.BaseClasses;
using DisappointmentCalculator.Enums;
using DisappointmentCalculator.Utilities;

using System.IO;

namespace DisappointmentCalculator.Data;

/// <summary>
/// Stores already parsed and daily grouped <see cref="Session"/>s for faster loading times.
/// </summary>
public static class SessionCache {
    /// <summary>
    /// The date after the last date present in the <see cref="SessionCache"/>. Anything before it shall not be loaded from the user's sessions but from the cache instead.
    /// </summary>
    public static DateTime LastCacheUpdate {
        get => File.Exists(lastCacheUpdateFile) ? DateTime.Parse(File.ReadAllText(lastCacheUpdateFile)) : default;
        private set => File.WriteAllText(lastCacheUpdateFile, value.ToString());
    }

    /// <summary>
    /// Get the path to all cached daily aggregates.
    /// </summary>
    public static List<(Guid, string)> GetSessionFiles() {
        if (!Directory.Exists(cachePath)) {
            Directory.CreateDirectory(cachePath);
        }

        List<(Guid, string)> result = [];
        string[] files = Directory.GetFiles(cachePath);
        foreach (string file in files) {
            if (file.EndsWith(".json")) {
                result.Add((new(Path.GetFileNameWithoutExtension(file)), file));
            }
        }
        return result;
    }

    /// <summary>
    /// Take discovered <see cref="Session"/>s that were not cached, and what can be cached, merge, and store in cache.
    /// </summary>
    public static void UpdateCache(SessionCollection uncached) {
        DateTime date = DateTime.Today; // Only cache data prior to today 0:00:00.000...
        Guid cacheUntil = GuidUtils.ToGuid(date.Year, date.Month, date.Day, GroupBy.Daily);
        SessionCollection aggregated = SessionDiscovery.GroupSessions(uncached, GroupBy.Daily);
        foreach (KeyValuePair<Guid, Session> session in aggregated) {
            if (session.Key >= cacheUntil) {
                break; // Because `aggregated` is sorted, there won't be more cacheable entries
            }

            session.Value.Save(Path.Combine(cachePath, session.Key.ToString() + ".json"));
        }

        LastCacheUpdate = date;
    }

    /// <summary>
    /// Where the session cache is located.
    /// </summary>
    const string cachePath = "Session Cache";

    /// <summary>
    /// Which file in the <see cref="cachePath"/> contains the last date present in cache.
    /// </summary>
    static readonly string lastCacheUpdateFile = Path.Combine(cachePath, "_Last.txt");
}
