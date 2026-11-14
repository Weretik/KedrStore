namespace Catalog.Application.Features.Attributes.Queries.GetCodeProducts;

public sealed class GetCodeProductsValidator : AbstractValidator<GetCodeProductsQuery>
{
    public GetCodeProductsValidator()
        => RuleFor(x => x.AliasId).GreaterThan(0).WithMessage("Некоректний код.");
}
