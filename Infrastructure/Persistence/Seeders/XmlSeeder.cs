using Application.Common.Settings;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Xml.Linq;

namespace Infrastructure.Persistence.Seeders
{
    public class XmlSeeder(HttpClient httpClient, ILogger<XmlSeeder> logger)
    {
        private readonly ILogger<XmlSeeder> _logger = logger;
        private readonly HttpClient _httpClient = httpClient;

        public async Task SeedAsync(AppDbContext context, ImageSettings settings)
        {
            _logger.LogInformation("📥 XML Seeding started");

            string baseUrl = settings.BaseUrl;

            if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
            {
                _logger.LogInformation("⚠️ Data already exists, seeding skipped");
                return;
            }

            var categories = LoadCategoriesFromXml("wwwroot/xml/category.xml");
            _logger.LogInformation("📦 Loaded {Count} categories from XML", categories.Count);

            await context.Categories.AddRangeAsync(categories.ToList());

            var products = await LoadProductsFromXml("wwwroot/xml/product.xml", baseUrl);
            _logger.LogInformation("📦 Loaded {Count} products from XML", products.Count);

            await context.Products.AddRangeAsync(products.ToList());

            await context.SaveChangesAsync();
            _logger.LogInformation("✅ XML Seeding finished");
        }

        private static List<Category> LoadCategoriesFromXml(string path)
        {
            var doc = XDocument.Load(path);

            return doc.Root!.Elements("category")
                .Select(c => new Category
                (
                    id: int.Parse(c.Element("id")!.Value.Trim()),
                    parentId: TryParseNullableInt(c.Element("parent")?.Value),
                    name: c.Element("name")!.Value.Trim()
                ))
                .GroupBy(c => c.Id)
                .Select(g => g.First()) // ⚠️ взять только первую из дубликатов
                .ToList();
        }

        private async Task<List<Product>> LoadProductsFromXml(string path, string baseUrl)
        {
            var doc = XDocument.Load(path);

            var productTasks = doc.Root!.Elements("product")
                .Select(async p =>
                {
                    var id = (int)p.Element("id")!;
                    var name = (string)p.Element("name")!;
                    var categoryId = (int)p.Element("category")!;
                    var manufacturer = (string)p.Element("manufacturer")!;
                    var price = decimal.Parse((string)p.Element("price")!, CultureInfo.InvariantCulture);
                    var photoUrl = await ResolvePhotoUrlAsync(id, baseUrl);

                    return new Product(id, name, manufacturer, price, photoUrl, categoryId);
                });

            return (await Task.WhenAll(productTasks)).ToList();
        }

        private async Task<string> ResolvePhotoUrlAsync(int idProduct, string baseUrl)
        {
            string fullUrl = $"{baseUrl}/images/furniture/{idProduct}.jpg";

            try
            {
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, fullUrl));
                if (response.IsSuccessStatusCode)
                    return fullUrl;
            }
            catch
            {
                _logger.LogWarning("❌ Photo not found for Product ID {Id}, using fallback image", idProduct);
            }

            return $"{baseUrl}/images/furniture/00000000.jpg";
        }
        private static int? TryParseNullableInt(string? input)
        {
            if (int.TryParse(input?.Trim(), out var result))
                return result;
            return null;
        }
    }
}
