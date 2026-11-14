namespace Content.Application.Features.Documents.Shared;

/// <summary>Both languages once each, with a title and a body within the limits.</summary>
public sealed class LegalDocumentTextsValidator : AbstractValidator<IReadOnlyList<LegalDocumentTextData>?>
{
    public LegalDocumentTextsValidator()
    {
        RuleFor(texts => texts)
            .Must(texts => texts is not null && texts.Count == LegalDocumentVersion.Languages.Count &&
                           LegalDocumentVersion.Languages.All(language =>
                               texts.Count(text => text?.Language == language) == 1))
            .WithMessage("Both 'uk' and 'ru' texts are required, once each.")
            .OverridePropertyName("texts");
        RuleForEach(texts => texts).ChildRules(text =>
        {
            text.RuleFor(t => t.Title).NotEmpty().MaximumLength(LegalDocumentText.TitleMaxLength);
            text.RuleFor(t => t.BodyHtml).NotEmpty().MaximumLength(LegalDocumentText.BodyHtmlMaxLength);
        }).When(texts => texts is not null).OverridePropertyName("texts");
    }
}
