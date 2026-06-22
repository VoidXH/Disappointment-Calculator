using System.IO;

namespace DisappointmentCalculator.Data;

/// <summary>
/// Deletes cache entries that are older than the current local day.
/// </summary>
static class CacheCleanup {
    /// <summary>
    /// Deletes files older than today and removes all empty directories.
    /// </summary>
    public static void DeleteEntriesBeforeToday(string directoryPath) {
        if (!Directory.Exists(directoryPath)) {
            return;
        }

        DateTime today = DateTime.Today;
        foreach (string file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)) {
            if (File.GetLastWriteTime(file) < today) {
                File.Delete(file);
            }
        }

        List<string> directories = [.. Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories)];
        directories.Sort((left, right) => right.Length.CompareTo(left.Length));
        foreach (string directory in directories) {
            if (!Directory.EnumerateFileSystemEntries(directory).Any()) {
                Directory.Delete(directory);
            }
        }
    }
}
