using System.Globalization;
using Churn.Core;

namespace Churn.Training;

// Writes training rows to a CSV file
public static class CsvExporter
{
    public static void Write(string path, IEnumerable<UserFeatures> rows)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using var writer = new StreamWriter(path);
        var c = CultureInfo.InvariantCulture; // always use '.' as decimal separator

        writer.WriteLine("UserId,CutoffDateUtc,DaysSinceLastEvent,SessionsLast7Days,SessionsLast30Days," +
                         "ActivityTrend,PurchasesLast30Days,PageViewsPerSession,AccountAgeDays,Churned");

        foreach (var r in rows)
        {
            writer.WriteLine(string.Join(",",
                r.UserId.ToString(c),
                r.CutoffDateUtc.ToString("yyyy-MM-dd", c),
                r.DaysSinceLastEvent.ToString(c),
                r.SessionsLast7Days.ToString(c),
                r.SessionsLast30Days.ToString(c),
                r.ActivityTrend.ToString(c),
                r.PurchasesLast30Days.ToString(c),
                r.PageViewsPerSession.ToString(c),
                r.AccountAgeDays.ToString(c),
                r.Churned ? "true" : "false"));
        }
    }
}