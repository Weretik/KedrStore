namespace Application.Catalog.DTOs;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Manufacturer { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public int CategoryId { get; init; }
    public string? Photo { get; init; }

}
