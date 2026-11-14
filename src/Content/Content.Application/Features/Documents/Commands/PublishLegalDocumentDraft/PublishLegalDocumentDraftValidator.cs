namespace Content.Application.Features.Documents.Commands.PublishLegalDocumentDraft;

public sealed class PublishLegalDocumentDraftValidator : AbstractValidator<PublishLegalDocumentDraftCommand>
{
    public PublishLegalDocumentDraftValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.EffectiveFrom).NotEmpty().OverridePropertyName("effectiveFrom");
    }
}
