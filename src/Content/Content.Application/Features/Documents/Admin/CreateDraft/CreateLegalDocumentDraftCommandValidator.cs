using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.CreateDraft;

public sealed class CreateLegalDocumentDraftCommandValidator : AbstractValidator<CreateLegalDocumentDraftCommand>
{
    public CreateLegalDocumentDraftCommandValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Texts).SetValidator(new LegalDocumentTextsValidator()).When(x => x.Texts is not null);
    }
}
