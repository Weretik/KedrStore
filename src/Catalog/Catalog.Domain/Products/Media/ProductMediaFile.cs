using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Slugs;
namespace Catalog.Domain.Products.Media;

/// <summary>A library file to link to a product: its Media ID and its current public URL.</summary>
public sealed record ProductMediaFile(long Id, string Url);
