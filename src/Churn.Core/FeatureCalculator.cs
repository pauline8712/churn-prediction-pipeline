namespace Churn.Core;

// Turns a user's raw events into one training row for a given cutoff date
public static class FeatureCalculator
{
    public const int LabelWindowDays = 30;

    public static UserFeatures Calculate(
        int userId,
        DateTime signupDateUtc,
        IReadOnlyList<UserEvent> events,
        DateTime cutoffUtc)
    {
        // ---------- FEATURES: only events strictly BEFORE the cutoff ----------
        var before = events.Where(e => e.TimestampUtc < cutoffUtc).ToList();

        // A session starts with a login
        var logins = before.Where(e => e.Type == EventType.Login).ToList();

        int sessionsLast7 = CountBetween(logins, cutoffUtc.AddDays(-7), cutoffUtc);
        int sessionsPrev7 = CountBetween(logins, cutoffUtc.AddDays(-14), cutoffUtc.AddDays(-7));
        int sessionsLast30 = CountBetween(logins, cutoffUtc.AddDays(-30), cutoffUtc);

        int purchasesLast30 = CountBetween(
            before.Where(e => e.Type == EventType.Purchase), cutoffUtc.AddDays(-30), cutoffUtc);

        int pageViewsLast30 = CountBetween(
            before.Where(e => e.Type == EventType.PageView), cutoffUtc.AddDays(-30), cutoffUtc);

        // If the user never did anything, count from signup
        var lastEventUtc = before.Count > 0
            ? before.Max(e => e.TimestampUtc)
            : signupDateUtc;

        // ---------- LABEL: only events in the 30 days AFTER the cutoff ----------
        bool activeAfterCutoff = events.Any(e =>
            e.TimestampUtc >= cutoffUtc &&
            e.TimestampUtc < cutoffUtc.AddDays(LabelWindowDays));

        return new UserFeatures
        {
            UserId = userId,
            CutoffDateUtc = cutoffUtc,

            DaysSinceLastEvent = (float)(cutoffUtc - lastEventUtc).TotalDays,
            SessionsLast7Days = sessionsLast7,
            SessionsLast30Days = sessionsLast30,
            ActivityTrend = sessionsLast7 - sessionsPrev7, // negative = slowing down
            PurchasesLast30Days = purchasesLast30,
            PageViewsPerSession = sessionsLast30 > 0 ? (float)pageViewsLast30 / sessionsLast30 : 0f,
            AccountAgeDays = (float)(cutoffUtc - signupDateUtc).TotalDays,

            Churned = !activeAfterCutoff
        };
    }

    // Counts events with fromUtc <= timestamp < toUtc
    private static int CountBetween(IEnumerable<UserEvent> events, DateTime fromUtc, DateTime toUtc) =>
        events.Count(e => e.TimestampUtc >= fromUtc && e.TimestampUtc < toUtc);
}