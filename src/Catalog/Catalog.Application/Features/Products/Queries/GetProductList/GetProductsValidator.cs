using Catalog.Application.Features.Products.Shared;

namespace Catalog.Application.Features.Products.Queries.GetProductList;

public sealed class GetProductsValidator : AbstractValidator<GetProductListQuery>
{
    public GetProductsValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит не може бути порожнім.")
            .SetValidator(new GetProductsRequestValidator());
    }
}
