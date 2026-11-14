using UnitTests.BuildingBlocks.Results;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace UnitTests.Catalog.Attributes;

/// <summary>Spec catalog/006 phase 3: manual values keep the automatic one for «Повернути автоматичне».</summary>
public sealed class AttributeManualEditTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);
    private static readonly AttributeValueId Bronze = AttributeValueId.Create(1);
    private static readonly AttributeValueId Chrome = AttributeValueId.Create(2);
    private static readonly AttributeValueId Gold = AttributeValueId.Create(3);

    private static ProductAttributeValue Imported()
        => ProductAttributeValue.CreateAutomatic(ProductId.From(7), AttributeId.Create(1), 0, Bronze, null, false,
            AttributeValueSource.Import, Now);

    [Fact]
    public void SetManually_KeepsTheAutomaticValue_AndRestoreBringsItBack()
    {
        var value = Imported();

        Assert.True(value.SetManually(Chrome, null, Now).Value);
        Assert.False(value.IsAutomatic);
        Assert.Equal((Bronze, AttributeValueSource.Import), (value.AutomaticValueId, value.AutomaticSource));

        // The import keeps refreshing the kept value but never touches the manual one.
        Assert.False(value.ApplyAutomatic(Gold, null, true, AttributeValueSource.Import, Now));
        Assert.Equal((Chrome, Gold), (value.ValueId, value.AutomaticValueId));

        Assert.True(value.RestoreAutomatic(Now));
        Assert.Equal((Gold, true, AttributeValueSource.Import, false, (AttributeValueSource?)null),
            (value.ValueId, value.IsConfirmed, value.Source, value.IsLocked, value.AutomaticSource));
    }

    [Fact]
    public void ManualOnlyValue_HasNothingToRestore()
    {
        var value = ProductAttributeValue.CreateManual(ProductId.From(7), AttributeId.Create(1), 1, Chrome, null, Now).Value;

        Assert.Null(value.AutomaticSource);
        Assert.False(value.RestoreAutomatic(Now));
        Assert.False(value.SetManually(Chrome, null, Now).Value);
    }

    [Fact]
    public void AliasDecision_IsConfirmedAndManual()
    {
        var alias = AttributeAlias.Create(AttributeId.Create(1), "general", "CP", Chrome, false, null, Now).Value;

        Assert.True(alias.Decide(null, Now));
        Assert.Equal(((AttributeValueId?)null, true, true), (alias.ValueId, alias.IsConfirmed, alias.IsEditedManually));
        Assert.False(alias.ApplyDictionary(Chrome, false, null, Now).Value);
    }

    [Theory]
    [InlineData("#3a3a3a", "#3A3A3A")]
    [InlineData(" ", null)]
    public void ValueSwatch_IsNormalized(string input, string? expected)
    {
        var value = AttributeValue.Create(AttributeId.Create(1), "GR", "Графіт", "hrafit", 1, Now).Value;

        value.EditManually("Графіт", null, 1, input, null, Now);

        Assert.Equal((expected, true), (value.SwatchHex, value.IsEditedManually));
    }

    [Fact]
    public void ValueSwatch_RejectsANonHexColor()
    {
        var value = AttributeValue.Create(AttributeId.Create(1), "GR", "Графіт", "hrafit", 1, Now).Value;

        var error = DomainAssert.Fails(value.EditManually("Графіт", null, 1, "graphite", null, Now));
        Assert.Equal("SwatchHex", error.Field);
    }
}
