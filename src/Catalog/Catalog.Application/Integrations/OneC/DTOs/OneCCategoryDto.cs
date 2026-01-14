namespace Catalog.Application.Integrations.OneC.DTOs;

public sealed record OneCCategoryDto(
    int CategoryId,
    int? ParentId,
    string CategoryName,
    string CategoryPath);
