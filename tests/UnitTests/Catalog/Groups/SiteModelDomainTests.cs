using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Entities.Groups;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace UnitTests.Catalog.Groups;

/// <summary>Spec catalog/007: a model's status and its texts.</summary>
public sealed class SiteModelDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Accept_PinsTheProposal_AndDropsItsKey_Once()
    {
        var model = ProductGroup.CreateAutomatic(ProductCategoryId.From(10), "Ручка", "ручка", "ruchka", Now);
        Assert.Equal(ProductGroupStatus.Proposed, model.Status);

        Assert.True(model.Accept("manager@example.com", Now));

        Assert.Equal(ProductGroupStatus.Accepted, model.Status);
        Assert.Null(model.GroupKey);
        Assert.Equal(("manager@example.com", Now), (model.AcceptedBy, model.AcceptedAt));
        Assert.False(model.Accept("admin@example.com", Now.AddHours(1)));
        Assert.Equal("manager@example.com", model.AcceptedBy);
    }

    [Fact]
    public void ManualModel_IsAcceptedAtOnce()
    {
        var model = ProductGroup.CreateManual(ProductCategoryId.From(10), "Ручка KEDR", "ruchka-kedr", Now);

        Assert.True(model.IsAccepted);
    }

    [Fact]
    public void Texts_AreTrimmed_CompleteWithShortAndSeoDescription_AndLimited()
    {
        var text = ProductGroupText.Create(ProductGroupId.Create(1), "uk", Now);
        Assert.False(text.IsComplete);

        Assert.True(text.Update("  Ручка ", null, "", "Опис для пошуку", Now));
        Assert.Equal(("Ручка", string.Empty), (text.ShortDescription, text.Description));
        Assert.True(text.IsComplete);
        Assert.False(text.Update("Ручка", " ", null, "Опис для пошуку", Now));

        Assert.Throws<DomainException>(() => text.Update(new string('x', 501), null, null, null, Now));
        Assert.Throws<DomainException>(() => ProductGroupText.Create(ProductGroupId.Create(1), "en", Now));
    }
}
