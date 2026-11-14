using UnitTests.BuildingBlocks.Results;
using Content.Application.Features.Contacts.Shared;
using Content.Application.Features.Contacts.Commands.UpdateCompanyContacts;
using Content.Domain.Contacts;

namespace UnitTests.Content;

public sealed class CompanyContactsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    private static CompanyContactsValues Values(string phone = "+380687135049", string telegram = "https://t.me/kedr") =>
        new(phone, " sales@classkedr.com.ua ", telegram, "+380687135049", "https://maps.app.goo.gl/x", " Харків ", "Харьков");

    [Fact]
    public void Create_TrimsAndKeepsTheSingletonId()
    {
        var contacts = CompanyContacts.Create(Values(), Now).Value;

        Assert.Equal(CompanyContacts.SingletonId, contacts.Id);
        Assert.Equal("sales@classkedr.com.ua", contacts.Email);
        Assert.Equal("Харків", contacts.CityUk);
        Assert.Equal(Now, contacts.UpdatedAt);
    }

    [Theory]
    [InlineData("0687135049")]
    [InlineData("+38068713504")]
    [InlineData("+48687135049")]
    public void Update_RejectsANonUkrainianOrShortPhone(string phone)
    {
        var contacts = CompanyContacts.Create(Values(), Now).Value;

        var error = DomainAssert.Fails(contacts.Update(Values(phone), Now, "manager"));
        Assert.Equal("SalesPhone", error.Field);
    }

    [Theory]
    [InlineData("https://t.me/kedr", true)]
    [InlineData("https://telegram.me/kedr", true)]
    [InlineData("http://t.me/kedr", false)]
    [InlineData("https://example.com/kedr", false)]
    public void TelegramUrl_MustBeAnHttpsTelegramLink(string url, bool valid)
        => Assert.Equal(valid, CompanyContacts.IsValidTelegramUrl(url));

    [Fact]
    public void Validator_ReportsEachInvalidFieldByItsJsonName()
    {
        var input = new CompanyContactsInput("068", "nope", "https://example.com", "+380687135049", "ftp://map",
            "", "Харьков");

        var result = new UpdateCompanyContactsValidator().Validate(new UpdateCompanyContactsCommand(input));

        Assert.Equal(["cityUk", "email", "mapUrl", "salesPhone", "telegramUrl"],
            result.Errors.Select(e => e.PropertyName).Distinct().Order());
    }
}
