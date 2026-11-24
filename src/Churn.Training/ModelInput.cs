using Microsoft.ML.Data;

namespace Churn.Training;

// Maps the columns in features.csv to properties ML.NET can read
public class ModelInput
{
    // Column 0 (UserId) is skipped on purpose: it must never be a feature

    [LoadColumn(1)]
    public string CutoffDateUtc { get; set; } = "";

    [LoadColumn(2)] public float DaysSinceLastEvent { get; set; }
    [LoadColumn(3)] public float SessionsLast7Days { get; set; }
    [LoadColumn(4)] public float SessionsLast30Days { get; set; }
    [LoadColumn(5)] public float ActivityTrend { get; set; }
    [LoadColumn(6)] public float PurchasesLast30Days { get; set; }
    [LoadColumn(7)] public float PageViewsPerSession { get; set; }
    [LoadColumn(8)] public float AccountAgeDays { get; set; }

    // ML.NET expects the value to predict to be called "Label"
    [LoadColumn(9), ColumnName("Label")]
    public bool Churned { get; set; }
}