namespace Content.Application.Features.Wholesale.Queries.GetPublicWholesalePage;

public sealed class GetPublicWholesalePageValidator : AbstractValidator<GetPublicWholesalePageQuery>
{
    public GetPublicWholesalePageValidator() =>
        RuleFor(x => x.Language).Must(language => WholesalePage.Languages.Contains(language))
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
}
