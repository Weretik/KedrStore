namespace Content.Application.Features.Wholesale.Public.Get;

public sealed class GetPublicWholesalePageQueryValidator : AbstractValidator<GetPublicWholesalePageQuery>
{
    public GetPublicWholesalePageQueryValidator() =>
        RuleFor(x => x.Language).Must(language => WholesalePage.Languages.Contains(language))
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
