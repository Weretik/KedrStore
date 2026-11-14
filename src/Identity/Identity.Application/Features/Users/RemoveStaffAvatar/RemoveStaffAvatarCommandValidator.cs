using FluentValidation;

namespace Identity.Application.Features.Users.RemoveStaffAvatar;

public sealed class RemoveStaffAvatarCommandValidator : AbstractValidator<RemoveStaffAvatarCommand>
{
    public RemoveStaffAvatarCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}
