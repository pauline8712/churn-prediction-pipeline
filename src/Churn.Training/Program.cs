using Churn.Data;
using Churn.Training;

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