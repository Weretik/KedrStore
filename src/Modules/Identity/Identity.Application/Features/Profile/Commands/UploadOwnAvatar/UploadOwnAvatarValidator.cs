using FluentValidation;
using Identity.Application.Features.Avatars;

namespace Identity.Application.Features.Profile.Commands.UploadOwnAvatar;

public sealed class UploadOwnAvatarValidator : AbstractValidator<UploadOwnAvatarCommand>
{
    public UploadOwnAvatarValidator()
    {
        RuleFor(x => x.Upload).NotNull().WithMessage(AvatarRules.MissingMessage)
            .OverridePropertyName(AvatarRules.FileField);
        RuleFor(x => x.Upload.Length).AvatarSize().When(x => x.Upload is not null)
            .OverridePropertyName(AvatarRules.FileField);
    }
}
