namespace Content.Application.Features.Redirects.Queries.ListAdminSiteRedirects;

public sealed class ListAdminSiteRedirectsValidator : AbstractValidator<ListAdminSiteRedirectsQuery>
{
    public const int MaxPageSize = 100;

    public ListAdminSiteRedirectsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).OverridePropertyName("page");
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize).OverridePropertyName("pageSize");
        RuleFor(x => x.Search).MaximumLength(200).OverridePropertyName("search");
    }
}
