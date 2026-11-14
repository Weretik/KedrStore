namespace Content.Application.Features.Documents.Queries.GetLegalDocumentVersions;

public sealed class GetLegalDocumentVersionsValidator : AbstractValidator<GetLegalDocumentVersionsQuery>
{
    public GetLegalDocumentVersionsValidator() => RuleFor(x => x.Kind).NotEmpty();
}
