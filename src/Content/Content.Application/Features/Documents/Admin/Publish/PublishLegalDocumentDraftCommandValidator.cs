namespace Content.Application.Features.Documents.Admin.Publish;

public sealed class PublishLegalDocumentDraftCommandValidator : AbstractValidator<PublishLegalDocumentDraftCommand>
{
    public PublishLegalDocumentDraftCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.EffectiveFrom).NotEmpty().OverridePropertyName("effectiveFrom");
    }
}
