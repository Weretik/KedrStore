using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.UpdateHomeFaqItem;

public sealed class UpdateHomeFaqItemValidator : AbstractValidator<UpdateHomeFaqItemCommand>
{
    public UpdateHomeFaqItemValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Texts).NotNull().SetValidator(new HomeFaqInputValidator());
    }
}
