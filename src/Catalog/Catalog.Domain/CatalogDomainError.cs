using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.Groups;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
namespace Catalog.Domain;

public sealed record CatalogDomainError(string Code, string Message) : IDomainError
{
}
