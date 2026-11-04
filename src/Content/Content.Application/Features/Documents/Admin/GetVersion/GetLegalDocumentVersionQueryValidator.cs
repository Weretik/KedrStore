namespace Content.Application.Features.Documents.Admin.GetVersion;

public sealed class GetLegalDocumentVersionQueryValidator : AbstractValidator<GetLegalDocumentVersionQuery>
{
    public GetLegalDocumentVersionQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
