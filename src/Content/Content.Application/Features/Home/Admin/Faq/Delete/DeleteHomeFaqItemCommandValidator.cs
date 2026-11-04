namespace Content.Application.Features.Home.Admin.Faq.Delete;

public sealed class DeleteHomeFaqItemCommandValidator : AbstractValidator<DeleteHomeFaqItemCommand>
{
    public DeleteHomeFaqItemCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
