namespace Application.Catalog.DTOs;

public sealed record CategoryDto(int Id, string Name, int? ParentId);
