namespace Catalog.Application.Integrations.OneC.DTOs;

public sealed record CategoryDto(
    int Id,
    string ProductTypeIdOneC,
    string Name,
    string Slug,
    int? ParentId,
    string Path);
