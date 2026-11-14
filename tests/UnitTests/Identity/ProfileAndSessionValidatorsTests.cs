using BuildingBlocks.Application.Validation;
using Identity.Application.Features.Auth.Commands.SessionLogin;
using Identity.Application.Features.Auth.Commands.SessionRefresh;
using Identity.Application.Features.Auth.Shared;
using Identity.Application.Features.Avatars;
using Identity.Application.Features.Profile.Commands.ChangeOwnName;
using Identity.Application.Features.Profile.Commands.ChangeOwnPassword;
using Identity.Application.Features.Profile.Commands.UploadOwnAvatar;
using Identity.Application.Features.Profile.Shared;
namespace UnitTests.Identity;

/// <summary>Own profile and mobile session input: shape checks only, account checks stay in the services.</summary>
public sealed class ProfileAndSessionValidatorsTests
{
    public ProfileAndSessionValidatorsTests() => UkrainianValidationMessages.Use();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void OwnName_CannotBeBlank(string name)
        => Assert.Equal("Вкажіть назву.",
            Assert.Single(new ChangeOwnNameValidator().Validate(new ChangeOwnNameCommand(name)).Errors).ErrorMessage);

    [Fact]
    public void OwnName_LimitCountsTheTrimmedName()
    {
        var validator = new ChangeOwnNameValidator();
        var longest = new string('я', ProfileRules.FullNameMaxLength);

        Assert.True(validator.Validate(new ChangeOwnNameCommand($"  {longest}  ")).IsValid);
        Assert.Equal("Ім’я — не більше 100 символів.",
            Assert.Single(validator.Validate(new ChangeOwnNameCommand(longest + "я")).Errors).ErrorMessage);
    }

    [Fact]
    public void OwnPassword_NeedsCurrentAndAStaffStrengthNewOne()
    {
        var result = new ChangeOwnPasswordValidator().Validate(new ChangeOwnPasswordCommand("", "short"));

        Assert.Equal(["CurrentPassword", "NewPassword"], result.Errors.Select(e => e.PropertyName));
    }

    [Fact]
    public void OwnAvatar_MissingFileIsReportedOnTheFileField()
    {
        var error = Assert.Single(new UploadOwnAvatarValidator().Validate(new UploadOwnAvatarCommand(null!)).Errors);

        Assert.Equal(AvatarRules.FileField, error.PropertyName);
        Assert.Equal(AvatarRules.MissingMessage, error.ErrorMessage);
    }

    [Fact]
    public void SessionLogin_RequiresTheRequestAndBothFields()
    {
        Assert.False(new SessionLoginValidator().Validate(new SessionLoginCommand(null!)).IsValid);
        Assert.Equal(2, new SessionLoginValidator().Validate(
            new SessionLoginCommand(new SessionLoginCommandRequest("", ""))).Errors.Count);
    }

    [Fact]
    public void SessionRefresh_LimitsTheTokenLength()
    {
        var validator = new SessionRefreshValidator();

        Assert.True(validator.Validate(new SessionRefreshCommand(
            new SessionRefreshRequest(new string('t', LoginInputRules.MaxRefreshTokenLength)))).IsValid);
        Assert.False(validator.Validate(new SessionRefreshCommand(
            new SessionRefreshRequest(new string('t', LoginInputRules.MaxRefreshTokenLength + 1)))).IsValid);
        Assert.False(validator.Validate(new SessionRefreshCommand(new SessionRefreshRequest(""))).IsValid);
    }
}
