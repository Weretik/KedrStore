using Catalog.Application.Shared;

namespace Catalog.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesValidator : AbstractValidator<GetCategoriesQuery>
{
    public GetCategoriesValidator()
    {
        RuleFor(x => x.Lang).SupportedLanguage();
        RuleFor(x => x.Filter).NotNull();
        RuleFor(x => x.Filter.ProductTypeId).GreaterThan(0).When(x => x.Filter?.ProductTypeId is not null)
            .WithMessage("Некоректний тип товару.");
    }
}
