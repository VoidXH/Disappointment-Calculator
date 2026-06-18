using DisappointmentCalculator.Data.Sessions.BaseClasses;

namespace DisappointmentCalculator.Data;

/// <summary>
/// Parsed AI sessions, always sorted by Guid.
/// </summary>
public partial class SessionCollection : SortedDictionary<Guid, Session> { }
