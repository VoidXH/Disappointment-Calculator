using DisappointmentCalculator.Data;
using DisappointmentCalculator.Data.Sessions;

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

    static readonly string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "events.jsonl");
}
