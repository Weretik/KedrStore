using FluentValidation;
using Identity.Application.Features.Avatars;

namespace Identity.Application.Features.Users.Commands.UploadStaffAvatar;

public sealed class UploadStaffAvatarValidator : AbstractValidator<UploadStaffAvatarCommand>
{
    public UploadStaffAvatarValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Upload).NotNull().WithMessage(AvatarRules.MissingMessage)
            .OverridePropertyName(AvatarRules.FileField);
        RuleFor(x => x.Upload.Length).AvatarSize().When(x => x.Upload is not null)
            .OverridePropertyName(AvatarRules.FileField);
    }
}
