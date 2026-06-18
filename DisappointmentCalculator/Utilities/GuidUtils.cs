using DisappointmentCalculator.Enums;

namespace DisappointmentCalculator.Utilities;

/// <summary>
/// Utility methods for Guid operations related to year-month keys.
/// </summary>
public static class GuidUtils {
    /// <summary>
    /// Creates a Guid representing a specific year and month in the format 000000-YYYY-MM01-0000-000000.
    /// </summary>
    /// <param name="year">The year (e.g. 2026)</param>
    /// <param name="month">The month (1-12)</param>
    /// <returns>A Guid representing the year-month key.</returns>
    public static Guid ToGuid(int year, int month) => new($"00000000-{year:0000}-{month:00}01-0000-000000000000");

    /// <summary>
    /// Creates a Guid representing a specific date in the format 000000-YYYY-MMDD-0000-000000.
    /// </summary>
    /// <param name="year">The year (e.g. 2026)</param>
    /// <param name="month">The month (1-12)</param>
    /// <param name="day">Day of the month</param>
    /// <returns>A Guid representing the date key.</returns>
    public static Guid ToGuid(int year, int month, int day) => new($"00000000-{year:0000}-{month:00}{day:00}-0000-000000000000");

    /// <summary>
    /// Creates a Guid representing a specific date, with the format depending on what to <paramref name="groupBy"/>.
    /// </summary>
    /// <param name="year">The year (e.g. 2026)</param>
    /// <param name="month">The month (1-12)</param>
    /// <param name="day">Day of the month</param>
    /// <param name="groupBy">Decides the set of unique items and their mapping</param>
    /// <returns>A Guid representing the date key.</returns>
    public static Guid ToGuid(int year, int month, int day, GroupBy groupBy) => groupBy switch {
        GroupBy.Monthly => ToGuid(year, month),
        GroupBy.Daily => ToGuid(year, month, day),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Extracts the year and month from a Guid created by <see cref="ToGuid"/>.
    /// </summary>
    /// <param name="guid">A Guid in the format 000000-YYYY-MMDD-0000-000000</param>
    /// <returns>A tuple of (year, month).</returns>
    public static DateTime ToDate(this Guid guid) =>
        new(int.Parse(guid.ToString()[9..13]), int.Parse(guid.ToString()[14..16]), int.Parse(guid.ToString()[16..18]));
}
