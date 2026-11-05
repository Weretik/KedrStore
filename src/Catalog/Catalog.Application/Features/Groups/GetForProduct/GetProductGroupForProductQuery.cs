using Catalog.Contracts.Groups;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.GetForProduct;

public sealed record GetProductGroupForProductQuery(int ProductId) : IQuery<Result<AdminProductGroupMembershipDto>>;
