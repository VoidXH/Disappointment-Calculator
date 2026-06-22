using System.IO;

namespace DisappointmentCalculator.Data;

/// <summary>
/// Raised when an AI session file cannot be loaded because another process still has it locked.
/// </summary>
public class SessionFileInUseException : IOException {
    public SessionFileInUseException(string filePath, IOException innerException)
        : base($"Session file is still in use: {filePath}", innerException) {
        FilePath = filePath;
    }

    /// <summary>
    /// The locked session file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Returns whether an IO error represents a Windows sharing or lock violation.
    /// </summary>
    public static bool IsFileInUse(IOException exception) {
        int errorCode = exception.HResult & 0xFFFF;
        return errorCode is 32 or 33;
    }
}
