using Microsoft.Data.Sqlite;
using System.IO;

using DisappointmentCalculator.Data.Sessions.BaseClasses;

namespace DisappointmentCalculator.Data.Sessions;

/// <summary>
/// Represents a single session of Google Antigravity.
/// Parses SQLite session databases from ~/.gemini/antigravity-cli/conversations.
/// </summary>
public class AntigravitySession : Session {
    /// <summary>
    /// Parses an Antigravity session database.
    /// </summary>
    /// <param name="filePath">Path to the SQLite database file to parse</param>
    public AntigravitySession(string filePath) {
        LastWriteTime = File.GetLastWriteTimeUtc(filePath);
        SessionStartTime = new DateTimeOffset(File.GetCreationTime(filePath)).ToUnixTimeMilliseconds();

        Dictionary<string, TokenUsage> metricsByModel = [];

        using (SqliteConnection connection = new($"Data Source={filePath}")) {
            connection.Open();
            using SqliteCommand cmd = new("SELECT data FROM gen_metadata ORDER BY idx;", connection);
            using SqliteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) {
                if (reader.IsDBNull(0)) continue;
                byte[] data = (byte[])reader.GetValue(0);

                try {
                    (string modelName, TokenUsage usage) = ParseProtobuf(data);
                    if (modelName != null && usage != null) {
                        // Normalize model name (e.g. gemini-3-flash-a -> gemini-3-flash)
                        if (modelName.EndsWith("-a")) {
                            modelName = modelName[..^2];
                        }

                        if (metricsByModel.TryGetValue(modelName, out TokenUsage existing)) {
                            existing.InputTokens += usage.InputTokens;
                            existing.OutputTokens += usage.OutputTokens;
                            existing.CacheReadTokens += usage.CacheReadTokens;
                            existing.CacheWriteTokens += usage.CacheWriteTokens;
                            existing.ReasoningTokens += usage.ReasoningTokens;
                        } else {
                            metricsByModel[modelName] = usage;
                        }
                    }
                } catch {
                    // Ignore malformed generation metadata steps
                }
            }
        }

        if (metricsByModel.Count == 0) {
            throw new InvalidDataException("No valid token usage metrics found in session.");
        }

        ModelMetrics = metricsByModel;
    }

    /// <summary>
    /// Gets all session database paths from the Antigravity conversations directory.
    /// </summary>
    /// <returns>An enumerable of tuples containing the session Guid and the database file path.</returns>
    public static IEnumerable<(Guid sessionId, string eventsFile)> GetSessionFiles(DateTime lastUpdate) {
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string conversationsDir = Path.Combine(homeDir, ".gemini", "antigravity-cli", "conversations");

        if (!Directory.Exists(conversationsDir)) {
            return [];
        }

        List<(Guid sessionId, string eventsFile)> result = [];
        foreach (string file in Directory.EnumerateFiles(conversationsDir, "*.db", SearchOption.TopDirectoryOnly)) {
            if (File.GetLastWriteTimeUtc(file) < lastUpdate) {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(file);
            if (Guid.TryParse(fileName, out Guid sessionId)) {
                result.Add((sessionId, file));
            }
        }

        return result;
    }

    /// <summary>
    /// Deletes locally stored Antigravity session database files.
    /// </summary>
    public static void WipeCache() {
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string conversationsDir = Path.Combine(homeDir, ".gemini", "antigravity-cli", "conversations");
        CacheCleanup.DeleteEntriesBeforeToday(conversationsDir);
    }

    /// <summary>
    /// Parses a protobuf-encoded byte array to extract the model name and token usage metrics.
    /// </summary>
    /// <param name="data">The protobuf-encoded byte array to parse</param>
    /// <returns>A tuple containing the model name and parsed token usage</returns>
    static (string modelName, TokenUsage usage) ParseProtobuf(byte[] data) {
        int index = 0;
        int limit = data.Length;
        string modelName = null;
        TokenUsage usage = null;

        while (index < limit) {
            ulong key = ReadVarint(data, ref index);
            int wireType = (int)(key & 0x7);
            int fieldNumber = (int)(key >> 3);

            if (fieldNumber == 1 && wireType == 2) {
                ulong length = ReadVarint(data, ref index);
                byte[] subBytes = new byte[length];
                Array.Copy(data, index, subBytes, 0, (int)length);
                index += (int)length;

                (modelName, usage) = ParseField1(subBytes);
            } else {
                SkipField(data, ref index, wireType);
            }
        }

        return (modelName, usage);
    }

    /// <summary>
    /// Parses field 19 (model name) and field 4 (token usage) from a protobuf sub-message.
    /// </summary>
    /// <param name="bytes">The protobuf-encoded sub-message bytes</param>
    /// <returns>A tuple containing the model name and parsed token usage</returns>
    static (string modelName, TokenUsage usage) ParseField1(byte[] bytes) {
        int index = 0;
        int limit = bytes.Length;
        string modelName = null;
        TokenUsage usage = null;

        while (index < limit) {
            ulong key = ReadVarint(bytes, ref index);
            int wireType = (int)(key & 0x7);
            int fieldNumber = (int)(key >> 3);

            if (fieldNumber == 19 && wireType == 2) {
                ulong len = ReadVarint(bytes, ref index);
                modelName = System.Text.Encoding.UTF8.GetString(bytes, index, (int)len);
                index += (int)len;
            } else if (fieldNumber == 4 && wireType == 2) {
                ulong len = ReadVarint(bytes, ref index);
                byte[] f4Bytes = new byte[len];
                Array.Copy(bytes, index, f4Bytes, 0, (int)len);
                index += (int)len;

                usage = ParseTokenUsage(f4Bytes);
            } else {
                SkipField(bytes, ref index, wireType);
            }
        }

        if (modelName == "gemini-3-flash-a") {
            modelName = "gemini-3.5-flash"; // Unknown if the free tier is just misnaming 3.5 in db while the CLI shows 3.5 or actually uses 3
        }
        return (modelName, usage);
    }

    /// <summary>
    /// Parses a protobuf-encoded TokenUsage message to extract token counts for various categories.
    /// </summary>
    /// <param name="bytes">The protobuf-encoded TokenUsage bytes</param>
    /// <returns>A populated TokenUsage instance</returns>
    static TokenUsage ParseTokenUsage(byte[] bytes) {
        int index = 0;
        int limit = bytes.Length;
        TokenUsage usage = new();

        while (index < limit) {
            ulong key = ReadVarint(bytes, ref index);
            int wireType = (int)(key & 0x7);
            int fieldNumber = (int)(key >> 3);

            if (wireType == 0) {
                ulong val = ReadVarint(bytes, ref index);
                switch (fieldNumber) {
                    case 2:
                        usage.InputTokens += (long)val;
                        break;
                    case 3:
                        usage.OutputTokens = (long)val;
                        break;
                    case 5: // For Antigravity, input is not total input
                        usage.InputTokens += (long)val;
                        usage.CacheReadTokens = (long)val;
                        break;
                    case 10:
                        usage.ReasoningTokens = (long)val;
                        break;
                }
            } else {
                SkipField(bytes, ref index, wireType);
            }
        }

        return usage;
    }

    /// <summary>
    /// Skips over a protobuf field based on its wire type, advancing the index accordingly.
    /// </summary>
    /// <param name="data">The byte array being parsed</param>
    /// <param name="index">Reference to the current index, advanced past the field</param>
    /// <param name="wireType">The protobuf wire type of the field to skip</param>
    static void SkipField(byte[] data, ref int index, int wireType) {
        if (wireType == 0) ReadVarint(data, ref index);
        else if (wireType == 1) index += 8;
        else if (wireType == 5) index += 4;
        else if (wireType == 2) {
            ulong len = ReadVarint(data, ref index);
            index += (int)len;
        }
    }

    /// <summary>
    /// Reads a protobuf varint from the byte array, advancing the index past the encoded value.
    /// </summary>
    /// <param name="data">The byte array containing the varint</param>
    /// <param name="index">Reference to the current index, advanced past the varint</param>
    /// <returns>The decoded varint value</returns>
    static ulong ReadVarint(byte[] data, ref int index) {
        ulong result = 0;
        int shift = 0;
        while (true) {
            byte b = data[index++];
            result |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0) {
                break;
            }
            shift += 7;
        }
        return result;
    }
}
