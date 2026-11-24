using System.Text.Json;
using Churn.Core;
using Microsoft.ML;

namespace Churn.Training;

// Saves every trained model as a new version and promotes it only if it beats the current one
public class ModelRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _directory;

    public ModelRegistry(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);
    }

    public string CurrentModelPath => Path.Combine(_directory, "model_current.zip");
    public string CurrentMetadataPath => Path.Combine(_directory, "model_current.json");

    public ModelMetadata? GetCurrent() =>
        File.Exists(CurrentMetadataPath)
            ? JsonSerializer.Deserialize<ModelMetadata>(File.ReadAllText(CurrentMetadataPath))
            : null;

    public bool SaveAndMaybePromote(MLContext ml, ITransformer model, DataViewSchema schema, ModelMetadata metadata)
    {
        // Next version number = number of saved versions + 1
        int version = Directory.GetFiles(_directory, "model_v*.zip").Length + 1;
        metadata = metadata with { Version = version };

        var modelPath = Path.Combine(_directory, $"model_v{version}.zip");
        ml.Model.Save(model, schema, modelPath);
        File.WriteAllText(Path.ChangeExtension(modelPath, ".json"), JsonSerializer.Serialize(metadata, JsonOptions));
        Console.WriteLine($"Saved {modelPath}");

        // Quality gate: only replace the current model if the new one is better
        var current = GetCurrent();
        if (current is not null && metadata.Auc <= current.Auc)
        {
            Console.WriteLine($"Not promoted: AUC {metadata.Auc:F3} is not better than current v{current.Version} ({current.Auc:F3})");
            return false;
        }

        File.Copy(modelPath, CurrentModelPath, overwrite: true);
        File.WriteAllText(CurrentMetadataPath, JsonSerializer.Serialize(metadata, JsonOptions));
        Console.WriteLine($"Promoted v{version} to current (AUC {metadata.Auc:F3})");
        return true;
    }
}