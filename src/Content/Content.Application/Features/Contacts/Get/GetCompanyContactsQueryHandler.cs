using Content.Application.Features.Contacts.Common;

namespace Content.Application.Features.Contacts.Get;

public sealed class GetCompanyContactsQueryHandler(IReadContentDbContext db)
    : IQueryHandler<GetCompanyContactsQuery, Result<CompanyContactsDetails>>
{
    public async ValueTask<Result<CompanyContactsDetails>> Handle(GetCompanyContactsQuery query,
        CancellationToken cancellationToken)
    {
        var contacts = await db.CompanyContacts.AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == CompanyContacts.SingletonId, cancellationToken);
        return contacts is null ? Result.NotFound() : Result.Success(CompanyContactsMapper.ToDetails(contacts));
    }
}
