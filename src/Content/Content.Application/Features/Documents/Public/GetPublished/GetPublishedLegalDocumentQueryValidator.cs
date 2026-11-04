namespace Content.Application.Features.Documents.Public.GetPublished;

public sealed class GetPublishedLegalDocumentQueryValidator : AbstractValidator<GetPublishedLegalDocumentQuery>
{
    public GetPublishedLegalDocumentQueryValidator()
    {
        RuleFor(x => x.Language).Must(language => language is not null &&
                                                  LegalDocumentVersion.Languages.Contains(language))
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
        RuleFor(x => x.Kind).IsInEnum();
    }
}
