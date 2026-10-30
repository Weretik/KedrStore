namespace Catalog.Contracts.Slugs;

/// <summary>Admin request: a new slug, or <c>Regenerate = true</c> to rebuild it from the current name.</summary>
public sealed record ChangeSlugRequest(string? Slug, bool Regenerate = false);

public sealed record ChangeSlugResponse(int Id, string Slug, string PreviousSlug);
