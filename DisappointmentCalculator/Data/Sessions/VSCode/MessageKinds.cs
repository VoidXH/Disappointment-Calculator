namespace DisappointmentCalculator.Data.Sessions.VSCode;

/// <summary>
/// The value of <c>kind</c> in a JSON line of a stored VS Code session.
/// </summary>
public enum MessageKinds {
    /// <summary>
    /// Initial system setup entry.
    /// </summary>
    System = 0,

    /// <summary>
    /// <c>v</c> is an object.
    /// </summary>
    VObject = 1,

    /// <summary>
    /// <c>v</c> is an array.
    /// </summary>
    VArray = 2,
}
