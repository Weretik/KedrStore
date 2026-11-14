namespace Catalog.Application.Features.OneCSync.Source;

public sealed record OneCCategoryDto(
    int CategoryId,
    int? ParentId,
    string CategoryName,
    string CategoryPath);
