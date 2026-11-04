using BuildingBlocks.Application.Contracts;
using Content.Application.Features.Editing;
using Content.Application.Features.Requisites.Common;

namespace Content.Application.Features.Requisites.Update;

/// <summary>
/// Replaces all requisites and, when any field changed, logs who changed what in the same save. Saving the
/// same values again changes nothing and adds no log entry.
/// </summary>
public sealed class UpdateCompanyRequisitesCommandHandler(
    IContentRepository<CompanyRequisites> requisitesRepository,
    IContentRepository<CompanyRequisitesChange> changesRepository,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateCompanyRequisitesCommand, Result<CompanyRequisitesDetails>>
{
    public async ValueTask<Result<CompanyRequisitesDetails>> Handle(UpdateCompanyRequisitesCommand command,
        CancellationToken cancellationToken)
    {
        var requisites = await requisitesRepository.GetByIdAsync(CompanyRequisites.SingletonId, cancellationToken);
        if (requisites is null)
            return Result.NotFound();

        var now = clock.GetUtcNow();
        var changes = requisites.Update(CompanyRequisitesMapper.ToValues(command.Input), now);
        if (changes.Count == 0)
            return Result.Success(CompanyRequisitesMapper.ToDetails(requisites));

        var userName = ContentEditor.Name(currentUser);
        // One SaveChanges writes the new values and their log entry together.
        await changesRepository.AddAsync(
            CompanyRequisitesChange.Record(changes, now, currentUser.UserId, userName), cancellationToken);
        return Result.Success(CompanyRequisitesMapper.ToDetails(requisites));
    }
}
