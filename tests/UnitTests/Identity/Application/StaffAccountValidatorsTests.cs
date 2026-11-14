using BuildingBlocks.Application.Validation;
using Identity.Application.Features.Users.Commands.ChangeStaffEmail;
using Identity.Application.Features.Users.Commands.ChangeStaffRole;
using Identity.Application.Features.Users.Commands.CreateStaff;
using Identity.Application.Features.Users.Commands.GrantStaffAccess;
using Identity.Application.Features.Users.Commands.SetStaffPassword;
using Identity.Application.Features.Users.Queries.SearchStaffCandidates;
using Identity.Application.Features.Users.Shared;
using Identity.Contracts.Authorization;

namespace UnitTests.Identity.Application;

/// <summary>Staff accounts: only staff roles, passwords of at least 8 characters, and a real email.</summary>
public sealed class StaffAccountValidatorsTests
{
    private const string RoleMessage = "Роль може бути лише Admin, Seo або Manager.";

    public StaffAccountValidatorsTests() => UkrainianValidationMessages.Use();

    [Theory]
    [InlineData(RoleNames.Admin)]
    [InlineData(RoleNames.Seo)]
    [InlineData(RoleNames.Manager)]
    public void CreateStaff_AcceptsEveryStaffRole(string role)
        => Assert.True(new CreateStaffValidator().Validate(
            new CreateStaffCommand("staff@kedr.test", "Олена Коваль", role, "secret-12")).IsValid);

    [Theory]
    [InlineData("User")]
    [InlineData("admin")]
    [InlineData("")]
    public void CreateStaff_RefusesARoleOutsideStaff(string role)
    {
        var result = new CreateStaffValidator().Validate(
            new CreateStaffCommand("staff@kedr.test", "Олена Коваль", role, "secret-12"));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(CreateStaffCommand.Role), error.PropertyName);
        Assert.Equal(RoleMessage, error.ErrorMessage);
    }

    [Fact]
    public void CreateStaff_RequiresARealEmailAndName()
    {
        var result = new CreateStaffValidator().Validate(
            new CreateStaffCommand("not-an-email", "", RoleNames.Manager, "secret-12"));

        Assert.Equal(["Email", "FullName"], result.Errors.Select(e => e.PropertyName));
    }

    [Theory]
    [InlineData("1234567", false)]
    [InlineData("12345678", true)]
    public void StaffPassword_NeedsAtLeastEightCharacters(string password, bool valid)
    {
        Assert.Equal(StaffPasswordRules.MinimumLength, 8);
        Assert.Equal(valid, new SetStaffPasswordValidator().Validate(
            new SetStaffPasswordCommand(Guid.NewGuid(), password)).IsValid);
    }

    [Fact]
    public void StaffPassword_HasAnUpperLimit()
        => Assert.False(new SetStaffPasswordValidator().Validate(
            new SetStaffPasswordCommand(Guid.NewGuid(), new string('x', 129))).IsValid);

    [Fact]
    public void Commands_OnAnAccount_RequireItsId()
    {
        Assert.False(new SetStaffPasswordValidator().Validate(new SetStaffPasswordCommand(Guid.Empty, "secret-12")).IsValid);
        Assert.False(new ChangeStaffRoleValidator().Validate(new ChangeStaffRoleCommand(Guid.Empty, RoleNames.Manager)).IsValid);
        Assert.False(new ChangeStaffEmailValidator().Validate(new ChangeStaffEmailCommand(Guid.Empty, "staff@kedr.test")).IsValid);
    }

    [Fact]
    public void GrantStaffAccess_ChecksRoleNameAndPassword()
    {
        var result = new GrantStaffAccessValidator().Validate(
            new GrantStaffAccessCommand(Guid.NewGuid(), "", RoleNames.User, "short"));

        Assert.Equal(["FullName", "Role", "Password"], result.Errors.Select(e => e.PropertyName));
    }

    [Theory]
    [InlineData("a", false)]
    [InlineData("  a  ", false)]
    [InlineData("ab", true)]
    public void SearchCandidates_NeedsTwoCharactersAfterTrim(string search, bool valid)
    {
        var result = new SearchStaffCandidatesValidator().Validate(new SearchStaffCandidatesQuery(search));

        Assert.Equal(valid, result.IsValid);
        if (!valid)
            Assert.Equal("Для пошуку введіть щонайменше 2 символи.", Assert.Single(result.Errors).ErrorMessage);
    }

    [Fact]
    public void StaffRoles_ListsStrongestFirst()
        => Assert.Equal([RoleNames.Admin, RoleNames.Seo, RoleNames.Manager], StaffRoles.All);
}
