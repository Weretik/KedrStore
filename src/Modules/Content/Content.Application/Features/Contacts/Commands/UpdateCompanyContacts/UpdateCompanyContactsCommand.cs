using Content.Application.Features.Contacts.Shared;

namespace Content.Application.Features.Contacts.Commands.UpdateCompanyContacts;

public sealed record UpdateCompanyContactsCommand(CompanyContactsInput Input)
    : ICommand<Result<CompanyContactsDetails>>;
