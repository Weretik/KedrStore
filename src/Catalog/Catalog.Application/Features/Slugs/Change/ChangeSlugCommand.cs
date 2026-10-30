using Catalog.Contracts.Slugs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Slugs.Change;

public sealed record ChangeSlugCommand(CatalogEntityType EntityType, int Id, ChangeSlugRequest Request)
    : ICommand<Result<ChangeSlugResponse>>;
