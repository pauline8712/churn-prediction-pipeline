namespace Churn.Core;

// One training example: a user's behaviour up to a cutoff date, plus the outcome after it
public record UserFeatures
{
    // Identifiers (NOT given to the model, only used to trace rows back)
    public int UserId { get; init; }
    public DateTime CutoffDateUtc { get; init; }

    // Features: computed only from events BEFORE the cutoff
    public float DaysSinceLastEvent { get; init; }
    public float SessionsLast7Days { get; init; }
    public float SessionsLast30Days { get; init; }
    public float ActivityTrend { get; init; }
    public float PurchasesLast30Days { get; init; }
    public float PageViewsPerSession { get; init; }
    public float AccountAgeDays { get; init; }

    // Label: true if the user had NO events in the 30 days AFTER the cutoff
    public bool Churned { get; init; }
}