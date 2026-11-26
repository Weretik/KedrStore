namespace Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;

/// <summary>Repeating a delete answers 404; nothing else changes.</summary>
public sealed class DeleteSiteRedirectHandler(IContentRepository<SiteRedirect> repository)
    : ICommandHandler<DeleteSiteRedirectCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteSiteRedirectCommand command, CancellationToken cancellationToken)
    {
        var redirect = await repository.GetByIdAsync(SiteRedirectId.FromStorage(command.Id), cancellationToken);
        if (redirect is null)
            return Result.NotFound();

        await repository.DeleteAsync(redirect, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}
