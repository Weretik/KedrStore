using Application.Catalog.Shared;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class ImportCatalogFromXmlCommandValidator : AbstractValidator<ImportCatalogFromXmlCommand>
{
    public ImportCatalogFromXmlCommandValidator(IValidator<UploadedFile> requestValidator)
    {
        RuleFor(сommand => сommand.Reuest.File)
            .SetValidator(requestValidator);

        RuleFor(command => command.Reuest.ProductTypeId)
            .InclusiveBetween(1, 2)
            .WithMessage("Тип продукту повинен бути в діапазоні 1–2.");
    }
}
