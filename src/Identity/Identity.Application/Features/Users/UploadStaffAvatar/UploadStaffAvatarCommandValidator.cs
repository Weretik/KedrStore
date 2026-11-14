using FluentValidation;
using Identity.Application.Features.Avatars;

namespace Identity.Application.Features.Users.UploadStaffAvatar;

public sealed class UploadStaffAvatarCommandValidator : AbstractValidator<UploadStaffAvatarCommand>
{
    public UploadStaffAvatarCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Upload).NotNull().WithMessage(AvatarRules.MissingMessage)
            .OverridePropertyName(AvatarRules.FileField);
        RuleFor(x => x.Upload.Length).AvatarSize().When(x => x.Upload is not null)
            .OverridePropertyName(AvatarRules.FileField);
    }
}
