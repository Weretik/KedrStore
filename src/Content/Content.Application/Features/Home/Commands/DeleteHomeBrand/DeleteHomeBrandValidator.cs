namespace Content.Application.Features.Home.Commands.DeleteHomeBrand;

public sealed class DeleteHomeBrandValidator : AbstractValidator<DeleteHomeBrandCommand>
{
    public DeleteHomeBrandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
