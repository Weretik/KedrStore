using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.Content.Save;

public sealed record SaveCategoryContentCommand(int CategoryId, string Language, SaveCategoryContentRequest Request, string UserId)
    : ICommand<Result<AdminCategoryContentItem>>;
