using Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;

namespace UnitTests.Catalog.RetailPrices;

/// <summary>Spec catalog/010 SC-RP05: the upload is checked before the file is read.</summary>
public sealed class ImportRetailPricesValidatorTests
{
    private readonly ImportRetailPricesValidator _validator = new();

    [Fact]
    public void AcceptsAnExcelFileUpToTheLimit()
        => Assert.True(_validator.Validate(Command(ImportRetailPricesValidator.MaxUploadBytes)).IsValid);

    [Theory]
    [InlineData(0)]
    [InlineData(ImportRetailPricesValidator.MaxUploadBytes + 1)]
    public void RefusesAnEmptyOrTooLargeFile_InUkrainian(long length)
    {
        var result = _validator.Validate(Command(length));

        var error = Assert.Single(result.Errors);
        Assert.Equal("file", error.PropertyName);
        Assert.Equal("Файл має бути Excel (.xlsx) розміром до 10 МБ.", error.ErrorMessage);
    }

    [Fact]
    public void RefusesAMissingFile_InUkrainian()
    {
        var result = _validator.Validate(new ImportRetailPricesCommand(null!, ProtectFromOneC: true));

        var error = Assert.Single(result.Errors);
        Assert.Equal("file", error.PropertyName);
        Assert.Equal("Виберіть файл з цінами.", error.ErrorMessage);
    }

    private static ImportRetailPricesCommand Command(long length)
        => new(new RetailPriceUpload("Цены_26.11.xlsx", length, () => Stream.Null), ProtectFromOneC: true);
}
