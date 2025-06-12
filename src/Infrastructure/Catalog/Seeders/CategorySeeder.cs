using System.Xml.Linq;
using Domain.Catalog.Entities;
using Domain.Catalog.Interfaces;
using Domain.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalog.Seeders;

public class CategorySeeder : ICategorySeeder
{
    private readonly CatalogDbContext _context;
    private readonly string _xmlPath;

    public CategorySeeder(CatalogDbContext context)
    {
        _context = context;
        _xmlPath = Path.Combine("wwwroot", "xml", "category.xml");
    }

    public async Task SeedAsync()
    {
        if (await _context.Categories.AnyAsync())
            return;

        if (!File.Exists(_xmlPath))
            throw new FileNotFoundException($"XML файл не знайдено: {_xmlPath}");

        var doc = XDocument.Load(_xmlPath);

        if (doc.Root == null || !doc.Root.Elements("category").Any())
            throw new InvalidDataException("В XML-файлі відсутні елементи <category>.");

        var categories = doc.Root
            .Elements("category")
            .Select(c =>
            {
                var id = int.Parse(c.Element("id")?.Value ?? "0");
                var name = c.Element("name")?.Value ?? string.Empty;
                var parentRaw = c.Element("parent")?.Value;
                var parentId = string.IsNullOrWhiteSpace(parentRaw) ? null : new CategoryId(int.Parse(parentRaw));

                return new { Id = id, Name = name, ParentId = parentId };
            })
            .DistinctBy(c => c.Id)
            .Select(c => Category.Create(new CategoryId(c.Id), c.Name, c.ParentId))
            .ToList();

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
    }
}
