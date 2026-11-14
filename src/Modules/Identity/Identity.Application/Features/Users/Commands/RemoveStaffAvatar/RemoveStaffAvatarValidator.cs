using FluentValidation;

namespace Identity.Application.Features.Users.Commands.RemoveStaffAvatar;

public sealed class RemoveStaffAvatarValidator : AbstractValidator<RemoveStaffAvatarCommand>
{
    public RemoveStaffAvatarValidator() => RuleFor(x => x.UserId).NotEmpty();
}
