namespace Content.Application.Features.Documents.Queries.GetPublishedLegalDocument;

public sealed class GetPublishedLegalDocumentValidator : AbstractValidator<GetPublishedLegalDocumentQuery>
{
    public GetPublishedLegalDocumentValidator()
    {
        RuleFor(x => x.Language).Must(language => language is not null &&
                                                  LegalDocumentVersion.Languages.Contains(language))
            .WithMessage("Language must be 'uk' or 'ru'.").OverridePropertyName("lang");
        RuleFor(x => x.Kind).NotEmpty();
    }
}
