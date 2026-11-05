namespace Content.Application.Features.SiteSeo.Public.List;

public sealed class ListPublicPageSeoQueryValidator : AbstractValidator<ListPublicPageSeoQuery>
{
    public ListPublicPageSeoQueryValidator() =>
        RuleFor(x => x.Language).Must(language => language is "uk" or "ru")
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
