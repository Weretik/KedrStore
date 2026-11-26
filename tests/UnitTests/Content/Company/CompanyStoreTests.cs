using UnitTests.BuildingBlocks.Results;
using Content.Domain.Contacts;
namespace UnitTests.Content.Company;

/// <summary>Spec content/012 R-02: the shop's address and opening hours for the LocalBusiness markup.</summary>
public sealed class CompanyStoreTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 0, 0, 0, TimeSpan.Zero);

    private static CompanyContactsValues Values(CompanyStoreValues? store) =>
        new("+380687135049", "sales@classkedr.com.ua", "https://t.me/kedr", "+380687135049",
            "https://maps.app.goo.gl/x", "Харків", "Харьков", store);

    [Fact]
    public void Store_KeepsTheTrimmedAddressAndHours_AndADayWithoutHoursIsOff()
    {
        var store = new CompanyStoreValues(" Крамниця Дверна фурнітура CLASS ", "вул. Ахієзерів, 46", "ул. Ахиезеров, 46",
            "Харківська обл.", "Харьковская обл.", "61038", "9:00", "16:00");

        var contacts = CompanyContacts.Create(Values(store), Now).Value;

        Assert.Equal("Крамниця Дверна фурнітура CLASS", contacts.Store.StoreName);
        Assert.Equal("61038", contacts.Store.PostalCode);
        Assert.Equal(new TimeOnly(9, 0), contacts.Store.WeekdaysOpens);
        Assert.Equal(new TimeOnly(16, 0), contacts.Store.WeekdaysCloses);
        Assert.Null(contacts.Store.SaturdayOpens);
        Assert.Null(contacts.Store.SundayCloses);
    }

    [Fact]
    public void NoStore_LeavesEveryFieldEmpty()
    {
        var contacts = CompanyContacts.Create(Values(null), Now).Value;

        Assert.Null(contacts.Store.StreetAddressUk);
        Assert.Null(contacts.Store.WeekdaysOpens);
    }

    [Fact]
    public void SaveWithoutAStore_KeepsTheShop_SoAnOlderAdminFormCannotClearIt()
    {
        var contacts = CompanyContacts.Create(Values(new CompanyStoreValues(StreetAddressUk: "вул. Ахієзерів, 46")), Now).Value;

        Assert.True(contacts.Update(Values(null), Now, "manager").IsSuccess);

        Assert.Equal("вул. Ахієзерів, 46", contacts.Store.StreetAddressUk);
    }

    [Theory]
    [InlineData("6103", null, null, "PostalCode")]
    [InlineData("61038", "9:00", null, "WeekdaysCloses")]
    [InlineData("61038", "nine", "16:00", "WeekdaysOpens")]
    [InlineData("61038", "16:00", "9:00", "WeekdaysCloses")]
    public void InvalidPostalCodeOrHours_AreRefusedByField(string postal, string? opens, string? closes, string field)
    {
        var store = new CompanyStoreValues(PostalCode: postal, WeekdaysOpens: opens, WeekdaysCloses: closes);
        var contacts = CompanyContacts.Create(Values(null), Now).Value;

        var error = DomainAssert.Fails(contacts.Update(Values(store), Now, "manager"));

        Assert.Equal(field, error.Field);
        Assert.Null(contacts.Store.PostalCode);
    }
}
