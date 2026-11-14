namespace Catalog.Application.Features.Groups.Queries.GetProductGroups;

public sealed class GetProductGroupsValidator : AbstractValidator<GetProductGroupsQuery>
{
    public GetProductGroupsValidator()
    {
        RuleFor(query => query.Request).NotNull();
        RuleFor(query => query.Request.CategoryId).GreaterThan(0)
            .When(query => query.Request?.CategoryId is not null).OverridePropertyName("categoryId");
        RuleFor(query => query.Request.Search).MaximumLength(100)
            .When(query => query.Request is not null).OverridePropertyName("search");
    }
}
