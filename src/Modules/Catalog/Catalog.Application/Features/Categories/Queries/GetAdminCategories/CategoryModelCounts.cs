namespace Catalog.Application.Features.Categories.Queries.GetAdminCategories;

/// <summary>Site models placed in one category: all of them, and those a person has not accepted yet («Не перевірена»).</summary>
public sealed record CategoryModelCounts(int Total, int Proposed);
