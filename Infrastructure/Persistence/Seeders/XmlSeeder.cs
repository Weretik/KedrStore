using Application.Common.Settings;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Infrastructure.Persistence.Seeders
{
    public class XmlSeeder(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task SeedAsync(AppDbContext context, ImageSettings settings)
        {
            string baseUrl = settings.BaseUrl;

            if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
                return;

            var categories = LoadCategoriesFromXml("wwwroot/xml/category.xml");
            await context.Categories.AddRangeAsync(categories.ToList());

            var products = await LoadProductsFromXml("wwwroot/xml/product.xml", baseUrl);
            await context.Products.AddRangeAsync(products.ToList());
            await context.SaveChangesAsync();
        }

        private static IEnumerable<Category> LoadCategoriesFromXml(string path)
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
                // Логировать
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
