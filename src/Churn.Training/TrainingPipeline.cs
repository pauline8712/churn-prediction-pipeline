using Churn.Core;
using Churn.Data;
using Microsoft.ML;

namespace Churn.Training;

// The full training pipeline: build dataset, train, evaluate, save and maybe promote
public static class TrainingPipeline
{
    public static void Run()
    {
        // Must match the simulator's dates
        var simulationEnd = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var simulationStart = simulationEnd.AddDays(-180);

        // Cutoffs every 14 days. Start late enough to have history,
        // stop 30 days before the end so the label window fits.
        var cutoffs = new List<DateTime>();
        for (var c = simulationStart.AddDays(60); c <= simulationEnd.AddDays(-30); c = c.AddDays(14))
            cutoffs.Add(c);

        Console.WriteLine($"Cutoffs: {string.Join(", ", cutoffs.Select(c => c.ToString("yyyy-MM-dd")))}");

        using var db = new ChurnDbContextFactory().CreateDbContext([]);

        var rows = DatasetBuilder.Build(db, cutoffs);
        CsvExporter.Write("data/features.csv", rows);

        int churned = rows.Count(r => r.Churned);
        Console.WriteLine();
        Console.WriteLine($"Wrote {rows.Count:N0} rows to data/features.csv");
        Console.WriteLine($"Churned: {churned:N0} ({100.0 * churned / rows.Count:F1}%)");

        // ---------- Load the CSV into ML.NET and split by time ----------
        var ml = new MLContext(seed: 42);

        var dataView = ml.Data.LoadFromTextFile<ModelInput>(
            "data/features.csv", separatorChar: ',', hasHeader: true);

        var allRows = ml.Data.CreateEnumerable<ModelInput>(dataView, reuseRowObject: false);

        // Last two cutoffs (2026-09-12 and 2026-09-26) are the test set
        var (trainRows, testRows) = DataSplitter.SplitByTime(allRows, firstTestCutoff: "2026-09-12");

        Console.WriteLine();
        Console.WriteLine($"Train: {trainRows.Count:N0} rows, {trainRows.Count(r => r.Churned):N0} churned");
        Console.WriteLine($"Test:  {testRows.Count:N0} rows, {testRows.Count(r => r.Churned):N0} churned");

        // ---------- Baseline ----------
        var baselineResult = Evaluation.FromPairs(
            testRows.Select(r => (r.Churned, Baseline.Predict(r))));
        Evaluation.Print($"Baseline (inactive > {Baseline.InactiveDaysThreshold} days)", baselineResult);

        // ---------- Train and evaluate the model ----------
        var trainData = ml.Data.LoadFromEnumerable(trainRows);
        var testData = ml.Data.LoadFromEnumerable(testRows);

        Console.WriteLine();
        Console.WriteLine("Training FastTree model...");
        var model = ModelTrainer.Train(ml, trainData);

        var predictions = model.Transform(testData);
        var metrics = ml.BinaryClassification.Evaluate(predictions, labelColumnName: "Label");

        Evaluation.Print("FastTree model", new ClassificationResult(
            metrics.PositivePrecision, metrics.PositiveRecall, metrics.F1Score, metrics.Accuracy));
        Console.WriteLine($"AUC:       {metrics.AreaUnderRocCurve:F3}");
        Console.WriteLine();
        Console.WriteLine(metrics.ConfusionMatrix.GetFormattedConfusionTable());

        // ---------- Compare decision thresholds ----------
        var scored = ml.Data.CreateEnumerable<ModelOutput>(predictions, reuseRowObject: false).ToList();

        Console.WriteLine();
        Console.WriteLine("Threshold | Precision | Recall | F1    | Flagged");
        Console.WriteLine("----------|-----------|--------|-------|--------");

        foreach (var threshold in new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f })
        {
            var r = Evaluation.FromPairs(scored.Select(s => (s.Actual, s.Probability >= threshold)));
            int flagged = scored.Count(s => s.Probability >= threshold);

            Console.WriteLine($"   {threshold:F1}    |  {r.Precision,6:P1}  | {r.Recall,6:P1} | {r.F1:F3} | {flagged,6}");
        }

        // ---------- Save the model and promote it if it is better ----------
        var chosen = Evaluation.FromPairs(
            scored.Select(s => (s.Actual, s.Probability >= ModelTrainer.DecisionThreshold)));

        var metadata = new ModelMetadata
        {
            TrainedAtUtc = DateTime.UtcNow,
            Threshold = ModelTrainer.DecisionThreshold,
            Auc = metrics.AreaUnderRocCurve,
            F1 = chosen.F1,
            Precision = chosen.Precision,
            Recall = chosen.Recall,
            TrainRows = trainRows.Count,
            TestRows = testRows.Count,
            Features = ModelTrainer.FeatureColumns,
        };

        Console.WriteLine();
        var registry = new ModelRegistry("models");
        registry.SaveAndMaybePromote(ml, model, trainData.Schema, metadata);
    }
}
