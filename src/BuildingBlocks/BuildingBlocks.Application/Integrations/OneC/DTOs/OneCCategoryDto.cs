namespace BuildingBlocks.Application.Integrations.OneC.DTOs;

public sealed record OneCCategoryDto(
    string CategoryId,
    string? ParentId,
    string CategoryName,
    string CategoryPath);
