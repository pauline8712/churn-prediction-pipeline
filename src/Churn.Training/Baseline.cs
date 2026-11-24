namespace Churn.Training;

// The simplest possible rule. The ML model must beat this to be worth using.
public static class Baseline
{
    public const float InactiveDaysThreshold = 7;

    public static bool Predict(ModelInput row) => row.DaysSinceLastEvent > InactiveDaysThreshold;
}