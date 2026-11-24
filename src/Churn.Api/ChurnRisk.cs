namespace Churn.Api;

// API response: the churn risk for one user, plus the main signals behind it
public record ChurnRisk(
    int UserId,
    float Probability,
    bool AtRisk,
    float Threshold,
    float DaysSinceLastEvent,
    float SessionsLast7Days,
    float SessionsLast30Days,
    float ActivityTrend);