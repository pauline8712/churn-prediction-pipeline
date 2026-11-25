using System.Text.Json;
using Churn.Core;

namespace Churn.Api;

// Paths and dates the API needs to make predictions
public record ModelSettings(string ModelPath, string MetadataPath, DateTime ReferenceDateUtc)
{
    // Read fresh every time, so a newly promoted model's metadata is picked up automatically
    public ModelMetadata ReadMetadata() =>
        JsonSerializer.Deserialize<ModelMetadata>(File.ReadAllText(MetadataPath))!;
}
