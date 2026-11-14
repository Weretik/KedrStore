using UnitTests.BuildingBlocks.Results;
using Content.Application.Features.Requisites.Shared;
using Content.Application.Features.Requisites.Commands.UpdateCompanyRequisites;
using Content.Domain.Requisites;

namespace UnitTests.Content;

public sealed class CompanyRequisitesTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private const string Iban = "UA303052990000026007025905216";

    private static CompanyRequisitesValues Values(string iban = Iban, string bankCode = "305299",
        string code = "2689400771", string legalName = " ФОП Овчинников Максим Володимирович ") =>
        new(legalName, code, "24800170000032534", iban, "ПриватБанк", bankCode);

    [Fact]
    public void Create_TrimsAndRecordsNoChanges()
    {
        var requisites = CompanyRequisites.Create(Values(), Now).Value;

        Assert.Equal(CompanyRequisites.SingletonId, requisites.Id);
        Assert.Equal("ФОП Овчинников Максим Володимирович", requisites.LegalName);
        Assert.Equal(Now, requisites.UpdatedAt);
    }

    [Fact]
    public void Update_ReportsOnlyTheChangedFields_WithOldAndNewValues()
    {
        var requisites = CompanyRequisites.Create(Values(), Now).Value;

        var changes = requisites.Update(Values(legalName: "ФОП Інший Власник"), Now.AddHours(1)).Value;

        var change = Assert.Single(changes);
        Assert.Equal("LegalName", change.Field);
        Assert.Equal("ФОП Овчинников Максим Володимирович", change.OldValue);
        Assert.Equal("ФОП Інший Власник", change.NewValue);
        Assert.Equal(Now.AddHours(1), requisites.UpdatedAt);
    }

    [Fact]
    public void Update_WithTheSameValues_ChangesNothing()
    {
        var requisites = CompanyRequisites.Create(Values(), Now).Value;

        var changes = requisites.Update(Values(iban: "UA30 3052 9900 0002 6007 0259 0521 6"), Now.AddHours(1)).Value;

        Assert.Empty(changes);
        Assert.Equal(Now, requisites.UpdatedAt);
    }

    [Theory]
    [InlineData("UA303052990000026007025905216", true)]
    [InlineData("UA313052990000026007025905216", false)]
    [InlineData("UA30305299000002600702590521", false)]
    [InlineData("PL303052990000026007025905216", false)]
    public void IsValidIban_ChecksShapeAndChecksum(string iban, bool valid)
        => Assert.Equal(valid, CompanyRequisites.IsValidIban(iban));

    [Fact]
    public void Update_RejectsABankCodeThatDoesNotMatchTheIban()
    {
        var requisites = CompanyRequisites.Create(Values(), Now).Value;

        var error = DomainAssert.Fails(requisites.Update(Values(bankCode: "300711"), Now));
        Assert.Equal("Content.Requisites.BankCode.Mismatch", error.Code);
        Assert.Equal("BankCode", error.Field);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567890")]
    public void IsValidCode_AcceptsEdrpouAndRnokpp(string code) => Assert.True(CompanyRequisites.IsValidCode(code));

    [Fact]
    public void Record_NeedsAtLeastOneChange()
        => Assert.Throws<ArgumentException>(() => CompanyRequisitesChange.Record([], Now, "1", "admin@kedr"));

    [Fact]
    public void Validator_NamesEachInvalidField()
    {
        var result = new UpdateCompanyRequisitesValidator().Validate(new UpdateCompanyRequisitesCommand(
            new CompanyRequisitesInput("", "123", "12a", "UA00", "", "12")));

        var fields = result.Errors.Select(error => error.PropertyName).Distinct().ToList();
        Assert.Equal(["legalName", "code", "registrationExtractNumber", "iban", "bankName", "bankCode"], fields);
    }

    [Fact]
    public void Validator_ReportsABankCodeMismatchOnTheBankCodeField()
    {
        var result = new UpdateCompanyRequisitesValidator().Validate(new UpdateCompanyRequisitesCommand(
            new CompanyRequisitesInput("ФОП", "2689400771", "1", Iban, "Банк", "300711")));

        var error = Assert.Single(result.Errors);
        Assert.Equal("bankCode", error.PropertyName);
    }
}
