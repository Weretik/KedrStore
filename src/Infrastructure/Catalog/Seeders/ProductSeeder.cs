using System.Globalization;
using System.Xml.Linq;
using Domain.Catalog.Entities;
using Domain.Catalog.Interfaces;
using Domain.Catalog.ValueObjects;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalog.Seeders;

public class ProductSeeder : IProductSeeder
{
    private readonly CatalogDbContext _context;
    private readonly string _xmlPath;

    public ProductSeeder(CatalogDbContext context)
    {
        _context = context;
        _xmlPath = Path.Combine("wwwroot", "xml", "product.xml");
    }

    public async Task SeedAsync()
    {
        if (await _context.Products.AnyAsync())
            return;

        if (!File.Exists(_xmlPath))
            throw new FileNotFoundException($"XML файл не знайдено: {_xmlPath}");

        var doc = XDocument.Load(_xmlPath);

        if (doc.Root == null || !doc.Root.Elements("product").Any())
            throw new InvalidDataException("В XML-файлі відсутні елементи <product>.");

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

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
    }
}

