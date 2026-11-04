using BuildingBlocks.Application.Contracts;
using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.Publish;

/// <summary>
/// Publishes a draft as the next number and archives the version it replaces, in one save. The «Редакція від»
/// date may not be earlier than the replaced version's date.
/// </summary>
public sealed class PublishLegalDocumentDraftCommandHandler(
    IContentRepository<LegalDocumentVersion> versions,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<PublishLegalDocumentDraftCommand, Result<LegalDocumentVersionDetails>>
{
    public async ValueTask<Result<LegalDocumentVersionDetails>> Handle(PublishLegalDocumentDraftCommand command,
        CancellationToken cancellationToken)
    {
        var draft = await versions.GetByIdAsync(command.Id, cancellationToken);
        if (draft is null)
            return Result.NotFound();
        if (draft.Status != LegalDocumentStatus.Draft)
            return Result.Conflict("This version is already published.");

        var kind = draft.Kind;
        var published = await versions.FirstOrDefaultAsync(
            new LegalDocumentVersionByStatusSpec(kind, LegalDocumentStatus.Published), cancellationToken);
        if (published?.EffectiveFrom > command.EffectiveFrom)
            return Result.Invalid(new ValidationError("effectiveFrom",
                $"The date may not be earlier than the current edition ({published.EffectiveFrom:yyyy-MM-dd}).",
                "Content.Document.EffectiveFrom.TooEarly", ValidationSeverity.Error));

        var lastNumber = await versions.ListAsync(new LegalDocumentNumbersSpec(kind), cancellationToken);
        published?.Archive();
        draft.Publish((lastNumber.Count == 0 ? 0 : lastNumber.Max()) + 1, command.EffectiveFrom, clock.GetUtcNow(),
            LegalDocumentEditor.Name(currentUser));
        await versions.SaveChangesAsync(cancellationToken);
        return Result.Success(LegalDocumentMapper.ToDetails(draft));
    }
}
