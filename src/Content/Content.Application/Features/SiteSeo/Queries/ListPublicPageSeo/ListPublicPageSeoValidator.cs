namespace Content.Application.Features.SiteSeo.Queries.ListPublicPageSeo;

public sealed class ListPublicPageSeoValidator : AbstractValidator<ListPublicPageSeoQuery>
{
    public ListPublicPageSeoValidator() =>
        RuleFor(x => x.Language).Must(language => language is "uk" or "ru")
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
