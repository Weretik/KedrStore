using Content.Application.Features.Requisites.Shared;

namespace Content.Application.Features.Requisites.Commands.UpdateCompanyRequisites;

public sealed record UpdateCompanyRequisitesCommand(CompanyRequisitesInput Input)
    : ICommand<Result<CompanyRequisitesDetails>>;
