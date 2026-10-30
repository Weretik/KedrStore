using System.Linq.Expressions;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetSalesList;

/// <summary>One product with its localized name and the price of the price type that applies to it.</summary>
internal sealed class SalesProductRow
{
    public ProductId Id { get; init; }
    public string Name { get; init; } = null!;
    public string ProductSlug { get; init; } = null!;
    public string Photo { get; init; } = null!;
    public ProductCategoryId? CategoryId { get; init; }
    public bool InStock { get; init; }
    public bool IsSale { get; init; }
    public bool IsNew { get; init; }
    public decimal? Price { get; init; }

    public static readonly Expression<Func<SalesProductRow, ProductListRowDto>> ToDto = row => new ProductListRowDto
    {
        Id = row.Id.Value,
        Name = row.Name,
        Photo = row.Photo,
        ProductSlug = row.ProductSlug,
        CategoryId = row.CategoryId.HasValue ? row.CategoryId.Value.Value : null,
        InStock = row.InStock,
        IsSale = row.IsSale,
        IsNew = row.IsNew,
        Price = row.Price
    };
}
