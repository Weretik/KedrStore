using System.Globalization;
using System.Xml.Linq;
using Domain.Catalog.Entities;
using Domain.Catalog.Interfaces;
using Domain.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Catalog.Seeders;

public class ProductSeeder(ILogger<ProductSeeder> logger) : ICatalogSeeder
{
    public async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var dbContext = serviceProvider.GetRequiredService<CatalogDbContext>();
        var xmlPath = Path.Combine("wwwroot", "xml", "product.xml");

        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Продукти вже існують - сидування пропущено.");
            return;
        }

        if (!File.Exists(xmlPath))
        {
            logger.LogError("XML-файл не знайдено: {Path}", xmlPath);
            throw new FileNotFoundException($"XML-файл не знайдено: {xmlPath}");
        }

        var doc = XDocument.Load(xmlPath);

        if (doc.Root == null || !doc.Root.Elements("product").Any())
        {
            logger.LogError("XML файл порожній або не містить елементів <product>.");
            throw new InvalidDataException("XML-файл не містить елементів <product>.");
        }

        var products = doc.Root
            .Elements("product")
            .Select(p =>
            {
                var id = int.Parse(p.Element("id")?.Value ?? "0");
                var name = p.Element("name")?.Value ?? string.Empty;
                var manufacturer = p.Element("manufacturer")?.Value ?? string.Empty;
                var price = decimal.Parse(p.Element("price")?.Value ?? "0", CultureInfo.InvariantCulture);
                var categoryId = int.Parse(p.Element("category")?.Value ?? "0");
                var photo = p.Element("photos")?.Element("photo")?.Value ?? "/product/default.jpg";

                return new
                {
                    Id = id,
                    Name = name,
                    Manufacturer = manufacturer,
                    Price = new Money(price),
                    CategoryId = categoryId,
                    Photo = photo
                };
            })
            .DistinctBy(p => p.Id)
            .Select(p => Product.Create(
                new ProductId(p.Id),
                p.Name,
                p.Manufacturer,
                p.Price,
                new CategoryId(p.CategoryId),
                p.Photo))
            .ToList();

        await dbContext.Products.AddRangeAsync(products, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Сидування продуктів завершено: {Count} записів.", products.Count);
    }
}
