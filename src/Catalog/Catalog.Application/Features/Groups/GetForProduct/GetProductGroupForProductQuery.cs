using Catalog.Contracts.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.GetForProduct;

public sealed record GetProductGroupForProductQuery(int ProductId) : IQuery<Result<AdminProductGroupMembershipDto>>;
