namespace Catalog.Application.Features.OneCSync.Source;

public sealed record CategoryDto(
    int Id,
    string ProductTypeIdOneC,
    string Name,
    int? ParentId,
    string Path);
