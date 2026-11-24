namespace Churn.Training;

public record ClassificationResult(double Precision, double Recall, double F1, double Accuracy);

// Computes and prints metrics for any yes/no predictor
public static class Evaluation
{
    public static ClassificationResult FromPairs(IEnumerable<(bool Actual, bool Predicted)> pairs)
    {
        int tp = 0, fp = 0, fn = 0, tn = 0;

        foreach (var (actual, predicted) in pairs)
        {
            if (actual && predicted) tp++;        // churned, and we caught it
            else if (!actual && predicted) fp++;  // false alarm
            else if (actual && !predicted) fn++;  // missed a churner
            else tn++;                            // stayed, and we said so
        }

        double precision = tp + fp == 0 ? 0 : (double)tp / (tp + fp);
        double recall = tp + fn == 0 ? 0 : (double)tp / (tp + fn);
        double f1 = precision + recall == 0 ? 0 : 2 * precision * recall / (precision + recall);
        double accuracy = (double)(tp + tn) / (tp + tn + fp + fn);

        return new ClassificationResult(precision, recall, f1, accuracy);
    }

    public static void Print(string name, ClassificationResult r)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {name} ---");
        Console.WriteLine($"Precision: {r.Precision:P1}");
        Console.WriteLine($"Recall:    {r.Recall:P1}");
        Console.WriteLine($"F1:        {r.F1:F3}");
        Console.WriteLine($"Accuracy:  {r.Accuracy:P1}");
    }
}