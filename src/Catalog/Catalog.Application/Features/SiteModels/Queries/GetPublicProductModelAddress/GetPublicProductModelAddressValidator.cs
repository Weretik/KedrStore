using Catalog.Application.Shared;

namespace Catalog.Application.Features.SiteModels.Queries.GetPublicProductModelAddress;

public sealed class GetPublicProductModelAddressValidator : AbstractValidator<GetPublicProductModelAddressQuery>
{
    public GetPublicProductModelAddressValidator() => RuleFor(x => x.Slug).LookupSlug();
}
