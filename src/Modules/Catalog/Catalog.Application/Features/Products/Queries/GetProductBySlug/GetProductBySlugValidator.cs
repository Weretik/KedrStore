using Catalog.Application.Shared;

namespace Catalog.Application.Features.Products.Queries.GetProductBySlug;

public sealed class GetProductBySlugValidator : AbstractValidator<GetProductBySlugQuery>
{
    public GetProductBySlugValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Slug).LookupSlug();
            RuleFor(x => x.Request.PriceTypeId).GreaterThan(0).WithMessage("Некоректний тип ціни.");
            RuleFor(x => x.Request.Lang).SupportedLanguage();
        });
    }
}
