using Churn.Core;
using Churn.Data;
using Microsoft.EntityFrameworkCore;

namespace Churn.Training;

// Loads users from the database and turns them into training rows for every cutoff date
public static class DatasetBuilder
{
    private const int MinAccountAgeDays = 14; // too new users have too little history
    private const int ActiveWithinDays = 30;  // only users who are still around at the cutoff
    private const int BatchSize = 500;

    public static List<UserFeatures> Build(ChurnDbContext db, IReadOnlyList<DateTime> cutoffs)
    {
        var rows = new List<UserFeatures>();
        int userCount = db.Users.Count();

        // Load users in batches so we never hold all 1.3M events in memory at once
        for (int skip = 0; skip < userCount; skip += BatchSize)
        {
            var users = db.Users
                .AsNoTracking()
                .Include(u => u.Events)
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(BatchSize)
                .AsSplitQuery()
                .ToList();

            foreach (var user in users)
            {
                foreach (var cutoff in cutoffs)
                {
                    if (!IsEligible(user, cutoff))
                        continue;

                    rows.Add(FeatureCalculator.Calculate(
                        user.Id, user.SignupDateUtc, user.Events, cutoff));
                }
            }

            Console.WriteLine($"Processed {Math.Min(skip + BatchSize, userCount)}/{userCount} users");
        }

        return rows;
    }

    // Exclude users who are too new or who had already left before the cutoff
    private static bool IsEligible(User user, DateTime cutoffUtc)
    {
        if ((cutoffUtc - user.SignupDateUtc).TotalDays < MinAccountAgeDays)
            return false;

        var windowStart = cutoffUtc.AddDays(-ActiveWithinDays);
        return user.Events.Any(e => e.TimestampUtc >= windowStart && e.TimestampUtc < cutoffUtc);
    }
}