namespace Catalog.Application.Features.Groups.Queries.GetProductGroup;

public sealed class GetProductGroupQueryValidator : AbstractValidator<GetProductGroupQuery>
{
    public GetProductGroupQueryValidator() => RuleFor(query => query.GroupId).GreaterThan(0);
}
