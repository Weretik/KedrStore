using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Update;

public sealed class UpdateHomeBrandCommandValidator : AbstractValidator<UpdateHomeBrandCommand>
{
    public UpdateHomeBrandCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Brand).NotNull().SetValidator(new HomeBrandInputValidator());
    }
}
