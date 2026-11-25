using Churn.Core;
using Churn.Data;
using Microsoft.EntityFrameworkCore;

namespace Churn.Api;

public record FeatureDrift(
    string Feature,
    double TrainingMean,
    double ProductionMean,
    double DriftScore,
    string Status);

public record MetricsResponse(
    int ModelVersion,
    DateTime ModelTrainedAtUtc,
    double ModelAuc,
    int PredictionCount,
    double AverageProbability,
    double AtRiskRate,
    bool DriftDetected,
    List<FeatureDrift> Features);

// Compares the features the model sees in production with what it saw during training
public class MonitoringService(ChurnDbContext db, ModelSettings settings)
{
    // Drift score = how many training standard deviations the production mean has moved
    private const double WarningLevel = 0.5;
    private const double DriftLevel = 1.0;

    // How to read each feature from a prediction log
    private static readonly Dictionary<string, Func<PredictionLog, float>> Selectors = new()
    {
        [nameof(PredictionLog.DaysSinceLastEvent)] = l => l.DaysSinceLastEvent,
        [nameof(PredictionLog.SessionsLast7Days)] = l => l.SessionsLast7Days,
        [nameof(PredictionLog.SessionsLast30Days)] = l => l.SessionsLast30Days,
        [nameof(PredictionLog.ActivityTrend)] = l => l.ActivityTrend,
        [nameof(PredictionLog.PurchasesLast30Days)] = l => l.PurchasesLast30Days,
        [nameof(PredictionLog.PageViewsPerSession)] = l => l.PageViewsPerSession,
        [nameof(PredictionLog.AccountAgeDays)] = l => l.AccountAgeDays,
    };

    public async Task<MetricsResponse> GetMetricsAsync(int hours)
    {
        var metadata = settings.ReadMetadata();
        var since = DateTime.UtcNow.AddHours(-hours);

        // Only predictions made by the current model, compared with that model's own baseline
        var logs = await db.PredictionLogs
            .AsNoTracking()
            .Where(l => l.PredictedAtUtc >= since && l.ModelVersion == metadata.Version)
            .ToListAsync();

        var features = new List<FeatureDrift>();

        if (logs.Count > 0)
        {
            foreach (var (name, baseline) in metadata.FeatureBaseline)
            {
                if (!Selectors.TryGetValue(name, out var select))
                    continue;

                var production = FeatureStatistics.From(logs.Select(select));

                double score = baseline.StdDev > 0
                    ? Math.Abs(production.Mean - baseline.Mean) / baseline.StdDev
                    : 0;

                string status = score >= DriftLevel ? "Drift"
                              : score >= WarningLevel ? "Warning"
                              : "OK";

                features.Add(new FeatureDrift(
                    name,
                    Math.Round(baseline.Mean, 2),
                    Math.Round(production.Mean, 2),
                    Math.Round(score, 2),
                    status));
            }
        }

        return new MetricsResponse(
            metadata.Version,
            metadata.TrainedAtUtc,
            Math.Round(metadata.Auc, 3),
            logs.Count,
            logs.Count > 0 ? Math.Round(logs.Average(l => l.Probability), 3) : 0,
            logs.Count > 0 ? Math.Round((double)logs.Count(l => l.AtRisk) / logs.Count, 3) : 0,
            features.Any(f => f.Status == "Drift"),
            features.OrderByDescending(f => f.DriftScore).ToList());
    }
}
