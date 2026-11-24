using System.Text.Json;
using Churn.Core;
using Churn.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;

namespace Churn.Api;

// Loads users from the database, computes features and asks the model for a churn probability
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

        return user is null ? null : Score(user, ReadThreshold());
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

        float threshold = ReadThreshold();

        return users
            .Select(u => Score(u, threshold))
            .Where(r => r.AtRisk)
            .OrderByDescending(r => r.Probability)
            .Take(top)
            .ToList();
    }

    private ChurnRisk Score(User user, float threshold)
    {
        // The SAME feature code as training: this is what prevents train/serve skew
        var features = FeatureCalculator.Calculate(
            user.Id, user.SignupDateUtc, user.Events, settings.ReferenceDateUtc);

        var prediction = pool.Predict(modelName: "churn", example: features);

        return new ChurnRisk(
            user.Id,
            prediction.Probability,
            prediction.Probability >= threshold,
            threshold,
            features.DaysSinceLastEvent,
            features.SessionsLast7Days,
            features.SessionsLast30Days,
            features.ActivityTrend);
    }

    // Read the threshold from the current model's metadata, so it follows the model
    private float ReadThreshold() =>
        JsonSerializer.Deserialize<ModelMetadata>(File.ReadAllText(settings.MetadataPath))!.Threshold;
}