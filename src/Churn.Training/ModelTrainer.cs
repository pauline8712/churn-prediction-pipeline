using Microsoft.ML;

namespace Churn.Training;

// Builds and trains the ML.NET pipeline
public static class ModelTrainer
{

        // Chosen from the threshold comparison: good balance between precision and recall
    public const float DecisionThreshold = 0.4f;
    // The only columns the model is allowed to see
    public static readonly string[] FeatureColumns =
    {
        
        nameof(ModelInput.DaysSinceLastEvent),
        nameof(ModelInput.SessionsLast7Days),
        nameof(ModelInput.SessionsLast30Days),
        nameof(ModelInput.ActivityTrend),
        nameof(ModelInput.PurchasesLast30Days),
        nameof(ModelInput.PageViewsPerSession),
        nameof(ModelInput.AccountAgeDays),
    };

    public static ITransformer Train(MLContext ml, IDataView trainData)
    {
        var pipeline = ml.Transforms
            // Combine all feature columns into one vector called "Features"
            .Concatenate("Features", FeatureColumns)
            // Gradient-boosted decision trees for yes/no classification
            .Append(ml.BinaryClassification.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features"));

        return pipeline.Fit(trainData);
    }
}