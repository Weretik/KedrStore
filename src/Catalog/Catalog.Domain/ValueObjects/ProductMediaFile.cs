namespace Catalog.Domain.ValueObjects;

/// <summary>A library file to link to a product: its Media ID and its current public URL.</summary>
public sealed record ProductMediaFile(long Id, string Url);
