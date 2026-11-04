using Content.Application.Features.Contacts.Common;

namespace Content.Application.Features.Contacts.Update;

public sealed record UpdateCompanyContactsCommand(CompanyContactsInput Input)
    : ICommand<Result<CompanyContactsDetails>>;
