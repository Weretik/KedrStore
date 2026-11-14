using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Queries.GetProductGroupForProduct;

public sealed record GetProductGroupForProductQuery(int ProductId) : IQuery<Result<AdminProductGroupMembershipDto>>;
