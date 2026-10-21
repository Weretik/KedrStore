using Sales.Domain.Orders.Entities;

namespace UnitTests.Sales.Domain;

public sealed class OrderDraftTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_HasIndependentDraftIdentityAndNoOrderNumber()
    {
        var draft = OrderDraft.Create("  Autumn order ", " Keep for later ", "customer-1",
            [OrderDraftLine.Create("101", 2)], Now);

        Assert.Equal("Autumn order", draft.Title);
        Assert.Equal("Keep for later", draft.Note);
        Assert.Equal("customer-1", draft.CounterpartyId);
        Assert.Single(draft.Lines);
        Assert.False(string.IsNullOrWhiteSpace(draft.Version));
        Assert.Equal(Now, draft.CreatedAtUtc);
    }

    [Fact]
    public void Update_ReplacesLinesAndAdvancesVersion()
    {
        var draft = OrderDraft.Create("First", null, "customer-1",
            [OrderDraftLine.Create("101", 2)], Now);
        var originalVersion = draft.Version;

        draft.Update("Second", "Note", "customer-2",
            [OrderDraftLine.Create("202", 3)], Now.AddMinutes(1));

        Assert.Equal("customer-2", draft.CounterpartyId);
        Assert.Equal("202", Assert.Single(draft.Lines).ProductId);
        Assert.NotEqual(originalVersion, draft.Version);
        Assert.Equal(Now.AddMinutes(1), draft.UpdatedAtUtc);
    }

    [Fact]
    public void Create_RejectsDuplicateProducts()
    {
        Assert.Throws<ArgumentException>(() => OrderDraft.Create("Name", null, "customer-1",
            [OrderDraftLine.Create("101", 1), OrderDraftLine.Create("101", 2)], Now));
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("101", 0)]
    public void CreateLine_RejectsInvalidProductOrQuantity(string productId, int quantity)
    {
        Assert.ThrowsAny<ArgumentException>(() => OrderDraftLine.Create(productId, quantity));
    }
}
