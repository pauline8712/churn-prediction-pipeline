using System.Text.Json;
using Churn.Core;
using Churn.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;

namespace Churn.Api;

// Loads users from the database, computes features, asks the model for a churn probability
// and logs every prediction for monitoring
public class ChurnScoringService(
    ChurnDbContext db,
    PredictionEnginePool<UserFeatures, ChurnPrediction> pool,
    ModelSettings settings)
{
    // Must match DatasetBuilder: the model was only trained on users active in the last 30 days
    private const int ActiveWithinDays = 30;

    public async Task<ChurnRisk?> ScoreUserAsync(int userId)
    {
        var reference = settings.ReferenceDateUtc;

        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Events.Where(e => e.TimestampUtc < reference))
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return null;

        var (risk, log) = Score(user, ReadMetadata());

        db.PredictionLogs.Add(log);
        await db.SaveChangesAsync();

        return risk;
    }

    public async Task<List<ChurnRisk>> GetAtRiskAsync(int top)
    {
        var reference = settings.ReferenceDateUtc;
        var windowStart = reference.AddDays(-ActiveWithinDays);

        // Only users who are still around, same population the model was trained on
        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Events.Any(e => e.TimestampUtc >= windowStart && e.TimestampUtc < reference))
            .Include(u => u.Events.Where(e => e.TimestampUtc >= windowStart && e.TimestampUtc < reference))
            .AsSplitQuery()
            .ToListAsync();

        var metadata = ReadMetadata();
        var scored = users.Select(u => Score(u, metadata)).ToList();

        // Log every prediction in a single round trip, not one insert per user
        db.PredictionLogs.AddRange(scored.Select(s => s.Log));
        await db.SaveChangesAsync();

        return scored
            .Select(s => s.Risk)
            .Where(r => r.AtRisk)
            .OrderByDescending(r => r.Probability)
            .Take(top)
            .ToList();
    }

    private (ChurnRisk Risk, PredictionLog Log) Score(User user, ModelMetadata metadata)
    {
        // The SAME feature code as training: this is what prevents train/serve skew
        var f = FeatureCalculator.Calculate(
            user.Id, user.SignupDateUtc, user.Events, settings.ReferenceDateUtc);

        var prediction = pool.Predict(modelName: "churn", example: f);
        bool atRisk = prediction.Probability >= metadata.Threshold;

        var risk = new ChurnRisk(
            user.Id,
            prediction.Probability,
            atRisk,
            metadata.Threshold,
            f.DaysSinceLastEvent,
            f.SessionsLast7Days,
            f.SessionsLast30Days,
            f.ActivityTrend);

        var log = new PredictionLog
        {
            UserId = user.Id,
            PredictedAtUtc = DateTime.UtcNow,
            ModelVersion = metadata.Version,
            Probability = prediction.Probability,
            AtRisk = atRisk,
            DaysSinceLastEvent = f.DaysSinceLastEvent,
            SessionsLast7Days = f.SessionsLast7Days,
            SessionsLast30Days = f.SessionsLast30Days,
            ActivityTrend = f.ActivityTrend,
            PurchasesLast30Days = f.PurchasesLast30Days,
            PageViewsPerSession = f.PageViewsPerSession,
            AccountAgeDays = f.AccountAgeDays,
        };

        return (risk, log);
    }

    // Threshold and version come from the current model's metadata, so they follow the model
    private ModelMetadata ReadMetadata() =>
        JsonSerializer.Deserialize<ModelMetadata>(File.ReadAllText(settings.MetadataPath))!;
}
