namespace Catalog.Application.Integrations.OneC.DTOs;

public sealed record CategoryDto(
    int Id,
    string ProductTypeIdOneC,
    string Name,
    int? ParentId,
    string Path);
