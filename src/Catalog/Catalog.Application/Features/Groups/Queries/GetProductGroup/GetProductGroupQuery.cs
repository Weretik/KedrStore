using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Queries.GetProductGroup;

public sealed record GetProductGroupQuery(int GroupId) : IQuery<Result<AdminProductGroupDto>>;
