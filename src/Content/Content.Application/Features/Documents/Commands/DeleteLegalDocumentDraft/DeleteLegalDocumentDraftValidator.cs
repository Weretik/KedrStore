namespace Content.Application.Features.Documents.Commands.DeleteLegalDocumentDraft;

public sealed class DeleteLegalDocumentDraftValidator : AbstractValidator<DeleteLegalDocumentDraftCommand>
{
    public DeleteLegalDocumentDraftValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
