using Content.Application.Features.Requisites.Common;

namespace Content.Application.Features.Requisites.Update;

public sealed record UpdateCompanyRequisitesCommand(CompanyRequisitesInput Input)
    : ICommand<Result<CompanyRequisitesDetails>>;
