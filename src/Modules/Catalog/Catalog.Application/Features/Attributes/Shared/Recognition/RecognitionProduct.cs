namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>A product as the recognizer reads it: ID, category, and the original 1C name (never changed).</summary>
public sealed record RecognitionProduct(int Id, int? CategoryId, string Name);
