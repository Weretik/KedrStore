namespace Content.Application.Features.Home.Commands.DeleteHomeFaqItem;

public sealed class DeleteHomeFaqItemValidator : AbstractValidator<DeleteHomeFaqItemCommand>
{
    public DeleteHomeFaqItemValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
