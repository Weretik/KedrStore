namespace Catalog.Application.Features.Shared;

public sealed record CategoryDto(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    string Path);
