using DisappointmentCalculator.Enums;
using DisappointmentCalculator.Utilities;

namespace DisappointmentCalculator.Data;

/// <summary>
/// Create metrics of imported measurements.
/// </summary>
public class DataAggregator(SessionCollection sessions, GroupBy groupBy) {
    /// <summary>
    /// All sessions loaded from AI CLIs, aggregated by the <see cref="GroupBy"/> rule.
    /// </summary>
    public SessionCollection Sessions { get; } = sessions;

    /// <summary>
    /// What buckets to create of sessions.
    /// </summary>
    public GroupBy GroupBy { get; } = groupBy;

    /// <summary>
    /// Combined total of the <see cref="Sessions"/> instead of summed by models.
    /// </summary>
    public Dictionary<Guid, TokenUsage> Aggregated => aggregated ??= Sessions.Collapse();
    Dictionary<Guid, TokenUsage> aggregated;

    /// <summary>
    /// Name of each aggregated session.
    /// </summary>
    public string[] Labels {
        get {
            if (labels != null) {
                return labels;
            }

            string dateFormat = GroupBy switch {
                GroupBy.Monthly => "yyyy. MMM",
                GroupBy.Daily => "yyyy. MM. dd.",
                _ => throw new NotImplementedException()
            };
            labels = [.. Aggregated.Select(x => x.Key.ToDate().ToString(dateFormat))];
            return labels;
        }
    }
    string[] labels;

    /// <summary>
    /// Total cost in US dollars for each aggregated time unit.
    /// </summary>
    public decimal[] Cost {
        get {
            if (cost != null) {
                return cost;
            }

            int entries = CostByModel.Values.FirstOrDefault()?.Length ?? 0;
            cost = new decimal[entries];
            foreach (decimal[] array in CostByModel.Values) {
                for (int i = 0; i < entries; i++) {
                    cost[i] += array[i];
                }
            }
            return cost;
        }
    }
    decimal[] cost;

    /// <summary>
    /// Get how many US dollars each model consumed in each recorded time unit. Time units are listed with the <see cref="Labels"/>.
    /// </summary>
    public Dictionary<string, decimal[]> CostByModel {
        get {
            if (costByModel != null) {
                return costByModel;
            }

            costByModel = [];
            foreach (ModelPricing model in UsedModels.Select(ModelDatabase.GetPricing)) {
                costByModel[model.Name] = [.. Sessions.Select(x => x.Value.GetCostForModel(model))];
            }
            return costByModel;
        }
    }
    Dictionary<string, decimal[]> costByModel;

    /// <summary>
    /// Get how many tokens each model consumed in each recorded time unit. Time units are listed with the <see cref="Labels"/>.
    /// </summary>
    public Dictionary<string, long[]> TokenUseByModel {
        get {
            if (tokenUseByModel != null) {
                return tokenUseByModel;
            }

            tokenUseByModel = [];
            foreach (string model in UsedModels) {
                string key = ModelDatabase.GetPricing(model).Name;
                tokenUseByModel[key] = [.. Sessions.Select(x => x.Value.ModelMetrics.TryGetValue(model, out TokenUsage usage) ? usage.TotalTokens : 0)];
            }
            return tokenUseByModel;
        }
    }
    Dictionary<string, long[]> tokenUseByModel;

    /// <summary>
    /// Total API duration in milliseconds for each aggregated time unit.
    /// </summary>
    public long[] TotalApiDurationMs => totalApiDurationMs ??= [.. Sessions.Select(x => x.Value.TotalApiDurationMs)];
    long[] totalApiDurationMs;

    /// <summary>
    /// Names of all models the user have ever used.
    /// </summary>
    public HashSet<string> UsedModels => usedModels ??= [.. Sessions.SelectMany(x => x.Value.ModelMetrics.Keys)];
    HashSet<string> usedModels;
}
