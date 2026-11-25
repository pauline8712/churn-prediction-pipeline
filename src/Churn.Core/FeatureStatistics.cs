namespace Churn.Core;

// Summary of how one feature's values are distributed
public record FeatureStatistics(double Mean, double StdDev, int Count)
{
    public static FeatureStatistics From(IEnumerable<float> values)
    {
        var list = values.Select(v => (double)v).ToList();
        if (list.Count == 0)
            return new FeatureStatistics(0, 0, 0);

        double mean = list.Average();
        double variance = list.Sum(v => (v - mean) * (v - mean)) / list.Count;

        return new FeatureStatistics(mean, Math.Sqrt(variance), list.Count);
    }
}
