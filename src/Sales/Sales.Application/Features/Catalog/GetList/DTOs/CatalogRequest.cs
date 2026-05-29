namespace Sales.Application.Features.Catalog.GetList.DTOs;

public sealed record CatalogRequest
{
    public string Lang { get; init; } = "uk";
    public Guid? IdentityUserId { get; init; }
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public bool? InStock { get; init; } = true;
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
