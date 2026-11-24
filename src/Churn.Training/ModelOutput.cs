using Microsoft.ML.Data;

namespace Churn.Training;

// What we read back from the model's predictions
public class ModelOutput
{
    [ColumnName("Label")]
    public bool Actual { get; set; }

    // The model's estimated probability that the user churns (0.0 – 1.0)
    public float Probability { get; set; }
}