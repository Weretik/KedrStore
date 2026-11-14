using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.CreateHomeBrand;

public sealed class CreateHomeBrandValidator : AbstractValidator<CreateHomeBrandCommand>
{
    public CreateHomeBrandValidator() =>
        RuleFor(x => x.Brand).NotNull().SetValidator(new HomeBrandInputValidator());
}
