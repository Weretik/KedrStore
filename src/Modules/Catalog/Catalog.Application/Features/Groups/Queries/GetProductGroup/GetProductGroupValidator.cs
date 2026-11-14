namespace Catalog.Application.Features.Groups.Queries.GetProductGroup;

public sealed class GetProductGroupValidator : AbstractValidator<GetProductGroupQuery>
{
    public GetProductGroupValidator() => RuleFor(query => query.GroupId).GreaterThan(0);
}
