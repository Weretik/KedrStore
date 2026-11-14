using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Commands.CreateLegalDocumentDraft;

public sealed class CreateLegalDocumentDraftValidator : AbstractValidator<CreateLegalDocumentDraftCommand>
{
    public CreateLegalDocumentDraftValidator()
    {
        RuleFor(x => x.Kind).NotEmpty();
        RuleFor(x => x.Texts).SetValidator(new LegalDocumentTextsValidator()).When(x => x.Texts is not null);
    }
}
