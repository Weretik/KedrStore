using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SaveCategoryContent;

public sealed record SaveCategoryContentCommand(int CategoryId, string Language, SaveCategoryContentRequest Request, string UserId)
    : ICommand<Result<AdminCategoryContentItem>>;
