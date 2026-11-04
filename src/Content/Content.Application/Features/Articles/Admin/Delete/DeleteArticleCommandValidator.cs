namespace Content.Application.Features.Articles.Admin.Delete;

public sealed class DeleteArticleCommandValidator : AbstractValidator<DeleteArticleCommand>
{
    public DeleteArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
