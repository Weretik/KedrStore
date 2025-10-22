namespace Application.Catalog.Commands.ImportCatalogFromXml;

public sealed class ImportCatalogFromXmlCommandValidator : AbstractValidator<ImportCatalogFromXmlCommand>
{
    public ImportCatalogFromXmlCommandValidator(IValidator<ICatalogFromXml> requestValidator)
    {
        RuleFor(сommand => сommand.Request).SetValidator(requestValidator);
    }
}
