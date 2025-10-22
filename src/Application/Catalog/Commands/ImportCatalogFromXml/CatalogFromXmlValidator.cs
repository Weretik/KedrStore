namespace Application.Catalog.Commands.ImportCatalogFromXml;

public sealed class CatalogFromXmlValidator : AbstractValidator<ICatalogFromXml>
{
    public CatalogFromXmlValidator()
    {
        RuleFor(x => x.FileName).NotEmpty()
            .Must(fn => string.Equals(Path.GetExtension(fn), ".xml", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Файл має бути .xml");

        RuleFor(x => x.FileSize).GreaterThan(0).WithMessage("Файл порожній");

        RuleFor(x => x.Content).NotNull().Must(s => s.CanRead)
            .WithMessage("Потік файлу нечитабельний");
    }
}
