using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Create;

public sealed class CreateHomeBrandCommandValidator : AbstractValidator<CreateHomeBrandCommand>
{
    public CreateHomeBrandCommandValidator() =>
        RuleFor(x => x.Brand).NotNull().SetValidator(new HomeBrandInputValidator());
}
