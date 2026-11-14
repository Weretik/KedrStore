using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.UpdateHomeBrand;

public sealed class UpdateHomeBrandValidator : AbstractValidator<UpdateHomeBrandCommand>
{
    public UpdateHomeBrandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Brand).NotNull().SetValidator(new HomeBrandInputValidator());
    }
}
