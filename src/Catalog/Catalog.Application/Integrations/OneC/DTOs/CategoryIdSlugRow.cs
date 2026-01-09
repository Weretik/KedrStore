using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.DTOs;

public sealed record CategoryIdSlugRow(ProductCategoryId Id, string Slug);
