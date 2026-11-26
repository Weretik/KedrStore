namespace Catalog.Contracts.Category;

/// <summary>A category made by hand (spec catalog/011 MC-01, MC-02); without <c>ParentId</c> it sits at the top.</summary>
public sealed record CreateCategoryRequest(string NameUk, string NameRu, int? ParentId, bool? IsShownOnSite);

/// <summary>Puts a category under another one, or at the top without <c>ParentId</c> (MC-03, MC-09).</summary>
public sealed record MoveCategoryRequest(int? ParentId);

/// <summary>Moves a category one place up or down among its neighbours (MC-10): <c>up</c> or <c>down</c>.</summary>
public sealed record ReorderCategoryRequest(string Direction);

/// <summary>Puts site models into a final category (MC-07).</summary>
public sealed record AddCategorySiteModelsRequest(IReadOnlyList<int> ModelIds);
