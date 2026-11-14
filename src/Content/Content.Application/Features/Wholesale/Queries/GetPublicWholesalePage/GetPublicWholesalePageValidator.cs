namespace Content.Application.Features.Wholesale.Queries.GetPublicWholesalePage;

public sealed class GetPublicWholesalePageValidator : AbstractValidator<GetPublicWholesalePageQuery>
{
    public GetPublicWholesalePageValidator() =>
        RuleFor(x => x.Language).Must(language => WholesalePage.Languages.Contains(language))
            .WithMessage("Мова може бути лише uk або ru.").OverridePropertyName("lang");
}
