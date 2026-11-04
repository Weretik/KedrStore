namespace Content.Application.Features.Documents.Admin.DeleteDraft;

public sealed class DeleteLegalDocumentDraftCommandValidator : AbstractValidator<DeleteLegalDocumentDraftCommand>
{
    public DeleteLegalDocumentDraftCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
