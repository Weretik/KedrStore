using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.CreateHomeFaqItem;

public sealed class CreateHomeFaqItemValidator : AbstractValidator<CreateHomeFaqItemCommand>
{
    public CreateHomeFaqItemValidator() =>
        RuleFor(x => x.Texts).NotNull().SetValidator(new HomeFaqInputValidator());
}
