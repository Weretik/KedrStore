using BuildingBlocks.Application.Validation;
using Identity.Application.Features.Auth.CookieLogin;
using Identity.Application.Features.Avatars;
using Identity.Application.Features.Users.UploadStaffAvatar;

namespace UnitTests.Identity.Application;

/// <summary>Sign-in and avatar input is checked on the server; a wrong password still answers 401 in the handler.</summary>
public sealed class IdentityInputValidatorsTests
{
    public IdentityInputValidatorsTests() => UkrainianValidationMessages.Use();

    [Fact]
    public void CookieLogin_RequiresEmailAndPassword_InUkrainian()
    {
        var result = new CookieLoginCommandValidator().Validate(new CookieLoginCommand("", ""));

        Assert.Equal(["Email", "Password"], result.Errors.Select(e => e.PropertyName));
        Assert.All(result.Errors, error => Assert.Equal("Заповніть це поле.", error.ErrorMessage));
    }

    [Fact]
    public void CookieLogin_AcceptsAnyWellSizedPair()
        => Assert.True(new CookieLoginCommandValidator().Validate(new CookieLoginCommand("not-an-email", "x")).IsValid);

    [Theory]
    [InlineData(0, AvatarRules.MissingMessage)]
    [InlineData(AvatarRules.MaxBytes + 1, AvatarRules.TooLargeMessage)]
    public void StaffAvatar_ReportsTheFileProblemOnTheFileField(long length, string message)
    {
        var result = new UploadStaffAvatarCommandValidator().Validate(
            new UploadStaffAvatarCommand(Guid.NewGuid(), new AvatarUpload(Stream.Null, length)));

        var error = Assert.Single(result.Errors);
        Assert.Equal(AvatarRules.FileField, error.PropertyName);
        Assert.Equal(message, error.ErrorMessage);
    }
}
