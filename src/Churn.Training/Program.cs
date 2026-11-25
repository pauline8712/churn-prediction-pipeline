using System.Globalization;
using Churn.Training;

// Default: train once and exit. With --schedule: keep retraining on a fixed interval.
if (!args.Contains("--schedule"))
{
    TrainingPipeline.Run();
    return;
}

var hours = double.TryParse(
    Environment.GetEnvironmentVariable("RETRAIN_INTERVAL_HOURS"),
    NumberStyles.Float, CultureInfo.InvariantCulture, out var h) ? h : 24;

var interval = TimeSpan.FromHours(hours);
Console.WriteLine($"Scheduled retraining every {interval.TotalMinutes:N0} minutes");

// PeriodicTimer waits one interval before the first tick,
// so we don't retrain right after the initial training has run
using var timer = new PeriodicTimer(interval);

while (await timer.WaitForNextTickAsync())
{
    Console.WriteLine();
    Console.WriteLine($"[{DateTime.UtcNow:u}] Starting scheduled retraining");

    try
    {
        TrainingPipeline.Run();
    }
    catch (Exception ex)
    {
        // A failed run must not stop future runs; the current model stays in place
        Console.WriteLine($"Retraining failed: {ex.Message}");
    }
}
