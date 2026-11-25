using Churn.Core;

namespace Churn.Training;

// Computes the training-time distribution of every feature the model uses
public static class FeatureBaseline
{
    public static Dictionary<string, FeatureStatistics> Compute(IReadOnlyList<ModelInput> rows) => new()
    {
        [nameof(ModelInput.DaysSinceLastEvent)] = FeatureStatistics.From(rows.Select(r => r.DaysSinceLastEvent)),
        [nameof(ModelInput.SessionsLast7Days)] = FeatureStatistics.From(rows.Select(r => r.SessionsLast7Days)),
        [nameof(ModelInput.SessionsLast30Days)] = FeatureStatistics.From(rows.Select(r => r.SessionsLast30Days)),
        [nameof(ModelInput.ActivityTrend)] = FeatureStatistics.From(rows.Select(r => r.ActivityTrend)),
        [nameof(ModelInput.PurchasesLast30Days)] = FeatureStatistics.From(rows.Select(r => r.PurchasesLast30Days)),
        [nameof(ModelInput.PageViewsPerSession)] = FeatureStatistics.From(rows.Select(r => r.PageViewsPerSession)),
        [nameof(ModelInput.AccountAgeDays)] = FeatureStatistics.From(rows.Select(r => r.AccountAgeDays)),
    };
}
