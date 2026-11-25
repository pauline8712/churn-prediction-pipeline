namespace Churn.Core;

// Information saved next to every trained model
public record ModelMetadata
{
    public int Version { get; init; }
    public DateTime TrainedAtUtc { get; init; }

    // Probability at or above this counts as "will churn"
    public float Threshold { get; init; }

    // Measured on the test set
    public double Auc { get; init; }
    public double F1 { get; init; }
    public double Precision { get; init; }
    public double Recall { get; init; }

    public int TrainRows { get; init; }
    public int TestRows { get; init; }
    public string[] Features { get; init; } = [];

    // How each feature looked in the training data (baseline for drift detection)
    public Dictionary<string, FeatureStatistics> FeatureBaseline { get; init; } = new();
}