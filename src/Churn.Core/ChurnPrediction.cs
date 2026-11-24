namespace Churn.Core;

// The model's output. Property names must match the columns ML.NET produces.
public class ChurnPrediction
{
    public bool PredictedLabel { get; set; }

    // Probability that the user churns within 30 days (0.0 – 1.0)
    public float Probability { get; set; }

    public float Score { get; set; }
}