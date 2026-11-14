using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
namespace Catalog.Domain.Products.Media;

/// <summary>What a linked library file is for a product. Stored as an integer.</summary>
public enum ProductMediaKind
{
    Photo = 1,
    Scheme = 2
}
