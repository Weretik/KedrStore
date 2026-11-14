namespace Content.Application.Features.Requisites.Queries.GetCompanyRequisitesHistory;

public sealed class GetCompanyRequisitesHistoryValidator : AbstractValidator<GetCompanyRequisitesHistoryQuery>
{
    public GetCompanyRequisitesHistoryValidator()
        => RuleFor(x => x.Limit).InclusiveBetween(1, GetCompanyRequisitesHistoryQuery.DefaultLimit);
}
