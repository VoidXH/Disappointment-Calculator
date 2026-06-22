using DisappointmentCalculator.Data;
using DisappointmentCalculator.Data.Sessions;
using DisappointmentCalculator.Data.Sessions.BaseClasses;
using DisappointmentCalculator.Enums;
using DisappointmentCalculator.Utilities;

namespace DisappointmentCalculator.Tests;

[TestClass]
public class SessionTests {
    [TestMethod]
    public void Session_Ctor_ParsesModelMetrics() {
        CopilotSession session = new(testDataPath);

        IReadOnlyDictionary<string, TokenUsage> modelMetrics = session.ModelMetrics;

        Assert.IsNotNull(modelMetrics);
        Assert.IsNotEmpty(modelMetrics, "Expected at least one model in model metrics");
    }

    [TestMethod]
    public void Session_Ctor_ParsesSessionStartTime() {
        CopilotSession session = new(testDataPath);

        Assert.IsGreaterThan(0, session.SessionStartTime, "Expected a valid session start time");
    }

    [TestMethod]
    public void Session_Ctor_ParsesTotalApiDurationMs() {
        CopilotSession session = new(testDataPath);

        Assert.IsGreaterThan(0, session.TotalApiDurationMs, "Expected a valid total API duration");
    }

    [TestMethod]
    public void Session_Ctor_ParsesTokenUsageMetrics() {
        CopilotSession session = new(testDataPath);

        foreach (var kvp in session.ModelMetrics) {
            var metrics = kvp.Value;
            Assert.IsNotNull(metrics, $"Expected metrics for model {kvp.Key}");
            Assert.IsGreaterThanOrEqualTo(0, metrics.InputTokens, $"Input tokens should be non-negative for model {kvp.Key}");
            Assert.IsGreaterThanOrEqualTo(0, metrics.OutputTokens, $"Output tokens should be non-negative for model {kvp.Key}");
            Assert.IsGreaterThanOrEqualTo(0, metrics.CacheReadTokens, $"Cache read tokens should be non-negative for model {kvp.Key}");
            Assert.IsGreaterThanOrEqualTo(0, metrics.CacheWriteTokens, $"Cache write tokens should be non-negative for model {kvp.Key}");
            Assert.IsGreaterThanOrEqualTo(0, metrics.ReasoningTokens, $"Reasoning tokens should be non-negative for model {kvp.Key}");
        }
    }

    [TestMethod]
    public void Session_Ctor_ContainsExpectedModel() {
        CopilotSession session = new(testDataPath);

        Assert.IsTrue(session.ModelMetrics.ContainsKey("VoidAI"), "Expected VoidAI model in model metrics");
    }

    [TestMethod]
    public void CodexSession_Ctor_ParsesTokenCountsByModel() {
        CodexSession session = new(codexTestDataPath);

        Assert.AreEqual(1782140900963, session.SessionStartTime);
        Assert.IsTrue(session.ModelMetrics.ContainsKey("gpt-5.5"), "Expected gpt-5.5 model in Codex metrics.");

        TokenUsage usage = session.ModelMetrics["gpt-5.5"];
        Assert.AreEqual(35799, usage.InputTokens);
        Assert.AreEqual(696, usage.OutputTokens);
        Assert.AreEqual(31488, usage.CacheReadTokens);
        Assert.AreEqual(94, usage.ReasoningTokens);
    }

    [TestMethod]
    public void ModelDatabase_GetPricing_ReturnsCopilotAutoModelWithUnknownCost() {
        ModelPricing pricing = ModelDatabase.GetPricing("auto");

        Assert.AreEqual("Auto-select (unknown cost)", pricing.Name);
        Assert.AreEqual("auto", pricing.LoggedName);
        Assert.AreEqual(0, pricing.InputPricing);
        Assert.AreEqual(0, pricing.OutputPricing);
        Assert.AreEqual(0, pricing.CacheReadPricing);
        Assert.AreEqual(0, pricing.CacheWritePricing);
    }

    [TestMethod]
    public void CachedSession_LoadsExportedCacheFileBackToCorrectMonth() {
        DateTime sessionDate = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);
        Guid expectedDailyKey = GuidUtils.ToGuid(2026, 5, 17, GroupBy.Daily);
        Guid expectedMonthlyKey = GuidUtils.ToGuid(2026, 5, 1, GroupBy.Monthly);
        string tempDirectory = Path.Combine(Path.GetTempPath(), nameof(CachedSession_LoadsExportedCacheFileBackToCorrectMonth), Guid.NewGuid().ToString());
        string cacheFilePath = Path.Combine(tempDirectory, expectedDailyKey + ".json");

        try {
            SessionCollection sourceSessions = new() {
                [Guid.NewGuid()] = new TestSession(sessionDate)
            };
            SessionCollection dailyCacheExport = SessionDiscovery.GroupSessions(sourceSessions, GroupBy.Daily);
            dailyCacheExport[expectedDailyKey].Save(cacheFilePath);

            SessionCollection loadedCache = new() {
                [expectedDailyKey] = new CachedSession(cacheFilePath)
            };
            SessionCollection monthly = SessionDiscovery.GroupSessions(loadedCache, GroupBy.Monthly);

            Assert.IsTrue(monthly.ContainsKey(expectedMonthlyKey), "Expected the exported cache file to load back into May 2026.");
        } finally {
            if (Directory.Exists(tempDirectory)) {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public void SessionFileInUseException_IsFileInUse_DetectsSharingAndLockViolations() {
        Assert.IsTrue(SessionFileInUseException.IsFileInUse(new IOException("Sharing violation.", 32)));
        Assert.IsTrue(SessionFileInUseException.IsFileInUse(new IOException("Lock violation.", 33)));
        Assert.IsFalse(SessionFileInUseException.IsFileInUse(new IOException("Other IO error.", 5)));
    }

    static readonly string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "events.jsonl");
    static readonly string codexTestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "codex-rollout.jsonl");

    sealed class TestSession : Session {
        public TestSession(DateTime lastWriteTime) {
            LastWriteTime = lastWriteTime;
            SessionStartTime = new DateTimeOffset(lastWriteTime).ToUnixTimeMilliseconds();
            ModelMetrics = new Dictionary<string, TokenUsage> {
                ["TestModel"] = new() {
                    InputTokens = 10,
                    OutputTokens = 5
                }
            };
        }
    }
}
