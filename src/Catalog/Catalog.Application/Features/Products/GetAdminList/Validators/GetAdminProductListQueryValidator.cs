using Catalog.Application.Features.Products.GetAdminList;

namespace Catalog.Application.Features.Products.GetAdminList.Validators;

public sealed class GetAdminProductListQueryValidator : AbstractValidator<GetAdminProductListQuery>
{
    public GetAdminProductListQueryValidator()
    {
        const int maxSearchTermLength = 100;
        const int maxPageSize = 100;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Request)
            .NotNull()
            .WithMessage("Request cannot be empty.");

        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.SearchTerm)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Search term cannot exceed {maxSearchTermLength} characters.");

            RuleFor(query => query.Request.Sort)
                .IsInEnum()
                .WithMessage("Invalid sort parameter.");

            RuleFor(query => query.Request.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be >= 1.");

            RuleFor(query => query.Request.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page size must be >= 1.")
                .LessThanOrEqualTo(maxPageSize)
                .WithMessage($"Page size cannot exceed {maxPageSize}.");

            RuleFor(query => query.Request.CategorySlug)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Category cannot exceed {maxSearchTermLength} characters.");

            RuleFor(query => query.Request.CategoryId)
                .GreaterThan(0)
                .When(query => query.Request.CategoryId.HasValue)
                .WithMessage("Invalid category.");

            RuleFor(query => query.Request.PriceFrom)
                .GreaterThanOrEqualTo(0)
                .When(query => query.Request.PriceFrom.HasValue)
                .WithMessage("Minimum price cannot be less than 0.");

            RuleFor(query => query.Request.PriceTo)
                .GreaterThanOrEqualTo(0)
                .When(query => query.Request.PriceTo.HasValue)
                .WithMessage("Maximum price cannot be less than 0.");

            RuleFor(query => query.Request)
                .Must(request => !request.PriceFrom.HasValue ||
                                 !request.PriceTo.HasValue ||
                                 request.PriceFrom <= request.PriceTo)
                .WithMessage("Minimum price cannot be greater than maximum price.");
        });
    }
}
