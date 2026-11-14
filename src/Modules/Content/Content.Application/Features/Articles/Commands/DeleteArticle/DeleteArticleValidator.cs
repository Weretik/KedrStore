namespace Content.Application.Features.Articles.Commands.DeleteArticle;

public sealed class DeleteArticleValidator : AbstractValidator<DeleteArticleCommand>
{
    public DeleteArticleValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
