namespace Churn.Core;

// One prediction made by the API, stored for monitoring.
// No foreign key to User on purpose: logs must survive even if users are deleted or re-simulated.
public class PredictionLog
{
    public long Id { get; set; }

    public int UserId { get; set; }
    public DateTime PredictedAtUtc { get; set; }
    public int ModelVersion { get; set; }

    // What the model answered
    public float Probability { get; set; }
    public bool AtRisk { get; set; }

    // The exact features the model saw (used to detect drift)
    public float DaysSinceLastEvent { get; set; }
    public float SessionsLast7Days { get; set; }
    public float SessionsLast30Days { get; set; }
    public float ActivityTrend { get; set; }
    public float PurchasesLast30Days { get; set; }
    public float PageViewsPerSession { get; set; }
    public float AccountAgeDays { get; set; }
}
