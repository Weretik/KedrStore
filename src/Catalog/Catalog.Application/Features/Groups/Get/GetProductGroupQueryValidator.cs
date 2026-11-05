namespace Catalog.Application.Features.Groups.Get;

public sealed class GetProductGroupQueryValidator : AbstractValidator<GetProductGroupQuery>
{
    public GetProductGroupQueryValidator() => RuleFor(query => query.GroupId).GreaterThan(0);
}
