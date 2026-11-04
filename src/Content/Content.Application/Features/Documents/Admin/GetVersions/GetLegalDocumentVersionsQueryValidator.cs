namespace Content.Application.Features.Documents.Admin.GetVersions;

public sealed class GetLegalDocumentVersionsQueryValidator : AbstractValidator<GetLegalDocumentVersionsQuery>
{
    public GetLegalDocumentVersionsQueryValidator() => RuleFor(x => x.Kind).IsInEnum();
}
