using Catalog.Application.Shared;

namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public sealed class ImportCatalogFromXmlCommandValidator : AbstractValidator<ImportCatalogFromXmlCommand>
{
    public ImportCatalogFromXmlCommandValidator(IValidator<UploadedFile> requestValidator)
    {
        RuleFor(сommand => сommand.Request.File)
            .SetValidator(requestValidator);

        RuleFor(command => command.Request.ProductTypeId)
            .InclusiveBetween(1, 2)
            .WithMessage("Тип продукту повинен бути в діапазоні 1–2.");
    }
}
