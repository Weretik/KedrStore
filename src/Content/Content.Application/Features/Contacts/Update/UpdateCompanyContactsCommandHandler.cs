using BuildingBlocks.Application.Contracts;
using Content.Application.Features.Editing;
using Content.Application.Features.Contacts.Common;

namespace Content.Application.Features.Contacts.Update;

/// <summary>Replaces all contacts at once; the row comes from the migration, so a missing one is 404.</summary>
public sealed class UpdateCompanyContactsCommandHandler(
    IContentRepository<CompanyContacts> repository,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateCompanyContactsCommand, Result<CompanyContactsDetails>>
{
    public async ValueTask<Result<CompanyContactsDetails>> Handle(UpdateCompanyContactsCommand command,
        CancellationToken cancellationToken)
    {
        var contacts = await repository.GetByIdAsync(CompanyContacts.SingletonId, cancellationToken);
        if (contacts is null)
            return Result.NotFound();

        contacts.Update(CompanyContactsMapper.ToValues(command.Input), clock.GetUtcNow(), ContentEditor.Name(currentUser));
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(CompanyContactsMapper.ToDetails(contacts));
    }
}
