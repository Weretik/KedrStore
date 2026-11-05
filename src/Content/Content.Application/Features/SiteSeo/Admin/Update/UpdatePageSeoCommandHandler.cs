using BuildingBlocks.Application.Contracts;
using Content.Application.Features.Editing;
using Content.Application.Features.SiteSeo.Common;

namespace Content.Application.Features.SiteSeo.Admin.Update;

/// <summary>The first save of a page creates its row; later saves replace the texts.</summary>
public sealed class UpdatePageSeoCommandHandler(
    IContentRepository<PageSeo> repository,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdatePageSeoCommand, Result<AdminPageSeo>>
{
    public async ValueTask<Result<AdminPageSeo>> Handle(UpdatePageSeoCommand command,
        CancellationToken cancellationToken)
    {
        var values = new PageSeoValues(command.TitleUk, command.TitleRu, command.DescriptionUk, command.DescriptionRu);
        var now = clock.GetUtcNow();
        var editor = ContentEditor.Name(currentUser);

        var seo = await repository.GetByIdAsync(command.Page, cancellationToken);
        if (seo is null)
        {
            seo = PageSeo.Create(command.Page, values, now, editor);
            await repository.AddAsync(seo, cancellationToken);
        }
        else
        {
            seo.Update(values, now, editor);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(PageSeoMapper.ToAdmin(seo.Page, seo));
    }
}
