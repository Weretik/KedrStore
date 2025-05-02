using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Persistence.Repositories
{
    public class ProductRepository(AppDbContext context) : IProductRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<List<Product>> GetAllProductsAsync()
            => await _context.Products.ToListAsync();

        public async Task<Product?> GetProductByIdAsync(int id)
            =>  await _context.Products.FindAsync(id);
        

        public async Task AddProductAsync(Product product)
            => await _context.Products.AddAsync(product);

        public Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            return Task.CompletedTask;
        }
        public Task DeleteProductAsync(Product product)
        {
            product.SoftDelete();
            _context.Products.Update(product);
            return Task.CompletedTask;
        }
    }
}
