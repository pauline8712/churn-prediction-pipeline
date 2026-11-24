namespace Churn.Training;

// Splits rows by time: older cutoffs for training, newer cutoffs for testing
public static class DataSplitter
{
    public static (List<ModelInput> Train, List<ModelInput> Test) SplitByTime(
        IEnumerable<ModelInput> rows, string firstTestCutoff)
    {
        var all = rows.ToList();

        // ISO dates (yyyy-MM-dd) sort correctly as plain strings
        var train = all.Where(r => string.CompareOrdinal(r.CutoffDateUtc, firstTestCutoff) < 0).ToList();
        var test = all.Where(r => string.CompareOrdinal(r.CutoffDateUtc, firstTestCutoff) >= 0).ToList();

        return (train, test);
    }
}