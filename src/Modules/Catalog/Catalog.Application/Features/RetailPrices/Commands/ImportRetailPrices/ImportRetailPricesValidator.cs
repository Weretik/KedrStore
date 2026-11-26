namespace Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;

public sealed class ImportRetailPricesValidator : AbstractValidator<ImportRetailPricesCommand>
{
    /// <summary>The 1C price list of about 1 600 products is under 200 KB; 10 MB leaves room without risk.</summary>
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    public ImportRetailPricesValidator()
    {
        RuleFor(x => x.File).NotNull().WithMessage("Виберіть файл з цінами.").OverridePropertyName("file");
        RuleFor(x => x.File.Length)
            .Must(length => length is > 0 and <= MaxUploadBytes)
            .When(x => x.File is not null)
            .WithMessage("Файл має бути Excel (.xlsx) розміром до 10 МБ.")
            .OverridePropertyName("file");
    }
}
