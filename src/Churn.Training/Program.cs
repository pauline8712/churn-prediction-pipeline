using Churn.Data;
using Churn.Training;
using Microsoft.ML;

// Must match the simulator's dates
var simulationEnd = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
var simulationStart = simulationEnd.AddDays(-180);

// Cutoffs every 14 days. Start late enough to have history,
// stop 30 days before the end so the label window fits.
var cutoffs = new List<DateTime>();
for (var c = simulationStart.AddDays(60); c <= simulationEnd.AddDays(-30); c = c.AddDays(14))
    cutoffs.Add(c);

Console.WriteLine($"Cutoffs: {string.Join(", ", cutoffs.Select(c => c.ToString("yyyy-MM-dd")))}");

using var db = new ChurnDbContextFactory().CreateDbContext(args);

var rows = DatasetBuilder.Build(db, cutoffs);
CsvExporter.Write("data/features.csv", rows);

int churned = rows.Count(r => r.Churned);
Console.WriteLine();
Console.WriteLine($"Wrote {rows.Count:N0} rows to data/features.csv");
Console.WriteLine($"Churned: {churned:N0} ({100.0 * churned / rows.Count:F1}%)");

// ---------- Load the CSV into ML.NET and split by time ----------
var ml = new Microsoft.ML.MLContext(seed: 42);

var dataView = ml.Data.LoadFromTextFile<ModelInput>(
    "data/features.csv", separatorChar: ',', hasHeader: true);

var allRows = ml.Data.CreateEnumerable<ModelInput>(dataView, reuseRowObject: false);

// Last two cutoffs (2026-09-12 and 2026-09-26) are the test set
var (trainRows, testRows) = DataSplitter.SplitByTime(allRows, firstTestCutoff: "2026-09-12");

Console.WriteLine();
Console.WriteLine($"Train: {trainRows.Count:N0} rows, {trainRows.Count(r => r.Churned):N0} churned");
Console.WriteLine($"Test:  {testRows.Count:N0} rows, {testRows.Count(r => r.Churned):N0} churned");