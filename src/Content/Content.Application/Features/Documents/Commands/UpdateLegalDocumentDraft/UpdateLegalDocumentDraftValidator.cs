using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Commands.UpdateLegalDocumentDraft;

public sealed class UpdateLegalDocumentDraftValidator : AbstractValidator<UpdateLegalDocumentDraftCommand>
{
    public UpdateLegalDocumentDraftValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Texts).SetValidator(new LegalDocumentTextsValidator());
    }
}
