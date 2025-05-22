using System;
using Microsoft.EntityFrameworkCore;
using StoreApp.Data.Abstract;

namespace StoreApp.Data.Concrete;

public class EFStoreRepository : IStoreRepository
{
    private readonly StoreDbContext _context;

    public EFStoreRepository(StoreDbContext context)
    {
        _context = context;
    }

    public IQueryable<Product> Products => _context.Products;
    public IQueryable<Category> Categories => _context.Categories;

    public void CreateProduct(Product product, List<int> categoryIds)
    {
        // Kategorileri veritabanından al ve ilişkilendir
        var categories = _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToList();

        product.Categories = categories;
        
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public void CreateProduct(Product entity)
    {
        throw new NotImplementedException();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id); // ; eklendi
    }

    public int GetProductCount(string category)
    {
        return string.IsNullOrEmpty(category)
            ? Products.Count()
            : Products
                .Include(p => p.Categories)
                .Count(p => p.Categories.Any(c => c.Url.ToLower() == category.ToLower()));
    }

    public IQueryable<Product> GetProductsByCategory(string category, int page, int pageSize)
    {
        // Tüm ürünleri al
        var products = Products;

        if (!string.IsNullOrEmpty(category))
        {
            var categoryUrl = category.ToLower();
            Console.WriteLine($"Filtering by category URL: {categoryUrl}");

            // ProductCategory ve Categories tablolarını birleştirerek doğrudan sorgu yap
            var productIdsInCategory = _context.Set<ProductCategory>()
                .Join(_context.Categories,
                      pc => pc.CategoryId,
                      c => c.Id,
                      (pc, c) => new { pc.ProductId, c.Url })
                .Where(x => x.Url.ToLower() == categoryUrl)
                .Select(x => x.ProductId)
                .Distinct();

            Console.WriteLine($"Product IDs in category '{categoryUrl}': {string.Join(", ", productIdsInCategory)}");

            products = products
                .Where(p => productIdsInCategory.Contains(p.Id));
        }

        var result = products
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        // Sorgu sonuçlarını kontrol et
        var productIds = result.Select(p => p.Id).ToList();
        Console.WriteLine($"Found products: {string.Join(", ", productIds)}");

        return result;
    }

    IEnumerable<Product> IStoreRepository.GetProductsByCategory(string category, int page, int pageSize)
    {
        return GetProductsByCategory(category, page, pageSize);
    }
}