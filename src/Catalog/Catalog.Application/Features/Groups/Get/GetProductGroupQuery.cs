using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Get;

public sealed record GetProductGroupQuery(int GroupId) : IQuery<Result<AdminProductGroupDto>>;
