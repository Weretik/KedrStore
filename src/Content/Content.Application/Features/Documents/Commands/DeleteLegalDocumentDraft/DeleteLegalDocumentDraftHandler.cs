namespace Content.Application.Features.Documents.Commands.DeleteLegalDocumentDraft;

/// <summary>Drops an unpublished draft; published and archived versions are kept forever (409).</summary>
public sealed class DeleteLegalDocumentDraftHandler(IContentRepository<LegalDocumentVersion> versions)
    : ICommandHandler<DeleteLegalDocumentDraftCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteLegalDocumentDraftCommand command, CancellationToken cancellationToken)
    {
        var version = await versions.GetByIdAsync(command.Id, cancellationToken);
        if (version is null)
            return Result.NotFound();
        if (!version.IsDraft)
            return Result.Conflict("Опубліковані редакції зберігаються, видалити можна лише чернетку.");

        await versions.DeleteAsync(version, cancellationToken);
        return Result.NoContent();
    }
}
