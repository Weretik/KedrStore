using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.UpdateDraft;

public sealed class UpdateLegalDocumentDraftCommandValidator : AbstractValidator<UpdateLegalDocumentDraftCommand>
{
    public UpdateLegalDocumentDraftCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Texts).SetValidator(new LegalDocumentTextsValidator());
    }
}
