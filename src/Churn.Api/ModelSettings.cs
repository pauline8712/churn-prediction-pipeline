namespace Churn.Api;

// Paths and dates the API needs to make predictions
public record ModelSettings(string ModelPath, string MetadataPath, DateTime ReferenceDateUtc);