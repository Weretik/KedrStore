namespace Application.Catalog.DTOs;

public record ProductDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Manufacturer { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required int CategoryId { get; init; }
    public string? Photo { get; init; }

}
