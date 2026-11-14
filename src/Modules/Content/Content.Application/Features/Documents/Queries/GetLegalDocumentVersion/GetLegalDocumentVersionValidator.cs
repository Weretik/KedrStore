namespace Content.Application.Features.Documents.Queries.GetLegalDocumentVersion;

public sealed class GetLegalDocumentVersionValidator : AbstractValidator<GetLegalDocumentVersionQuery>
{
    public GetLegalDocumentVersionValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
