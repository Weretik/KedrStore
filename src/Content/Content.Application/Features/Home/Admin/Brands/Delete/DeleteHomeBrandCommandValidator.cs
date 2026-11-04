namespace Content.Application.Features.Home.Admin.Brands.Delete;

public sealed class DeleteHomeBrandCommandValidator : AbstractValidator<DeleteHomeBrandCommand>
{
    public DeleteHomeBrandCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
