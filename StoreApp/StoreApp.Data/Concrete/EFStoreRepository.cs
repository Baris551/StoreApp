using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StoreApp.Data.Abstract;

namespace StoreApp.Data.Concrete;

public class EFStoreRepository : IStoreRepository
{
    private readonly StoreDbContext _context;
    private readonly ILogger<EFStoreRepository> _logger;

    public EFStoreRepository(StoreDbContext context, ILogger<EFStoreRepository> logger)
    {
        _context = context; // Veritabanı bağlantısını başlatır
        _logger = logger;   // Loglama nesnesini başlatır
    }

    public IQueryable<Product> Products => _context.Products.AsQueryable();
    public IQueryable<Category> Categories => _context.Categories.AsQueryable();

    // Yeni bir ürün oluşturur ve isteğe bağlı olarak kategorilerle ilişkilendirir (asenkron)
    public async Task CreateProductAsync(Product product, List<int> categoryIds = null)
    {
        try
        {
            // Eğer kategori ID'leri verilmişse, bu kategorileri bul ve ürüne ekle
            if (categoryIds != null && categoryIds.Any())
            {
                // Veritabanından verilen ID'lere sahip kategorileri getir
                var categories = await _context.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToListAsync();
                product.Categories = categories; // Ürüne kategorileri bağla
            }

            // Ürünü veritabanına ekle
            _context.Products.Add(product);
            // Değişiklikleri veritabanına kaydet (asenkron)
            await _context.SaveChangesAsync();
            // Başarıyla kaydedildiğini logla
            _logger.LogInformation($"Product created with ID: {product.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating product.");
            throw;
        }
    }

    // Yeni bir ürün oluşturur (senkron versiyonu, asenkron metodu çağırır)
    public void CreateProduct(Product entity)
    {
        // Asenkron metodu senkron şekilde çalıştırır
        CreateProductAsync(entity, null).GetAwaiter().GetResult();
    }

    // Belirtilen ID'ye sahip ürünü getirir (kategorileriyle birlikte)
    public async Task<Product> GetProductByIdAsync(int id)
    {
        // Ürünü ve ilişkili kategorilerini veritabanından getir
        var product = await _context.Products
            .Include(p => p.Categories) // Kategorileri de dahil et
            .FirstOrDefaultAsync(p => p.Id == id); // ID'ye göre ürünü bul

        // Eğer ürün bulunamazsa, uyarı logu yaz
        if (product == null)
        {
            _logger.LogWarning($"Product with ID {id} not found.");
        }

        return product;
    }

    // Belirtilen kategorideki ürün sayısını getirir (asenkron)
    public async Task<int> GetProductCountAsync(string category)
    {
        try
        {
            // Eğer kategori belirtilmemişse, tüm ürünlerin sayısını döndür
            if (string.IsNullOrEmpty(category))
            {
                return await _context.Products.CountAsync();
            }

            // Belirtilen kategoriye sahip ürünlerin sayısını getir
            // EF.Functions.Like ile kategori URL'si büyük/küçük harf duyarsız karşılaştırılır
            return await _context.Products
                .Where(p => p.Categories.Any(c => EF.Functions.Like(c.Url.ToLower(), category.ToLower())))
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while getting product count for category: {category}");
            throw;
        }
    }

    // Kategoriye göre ürünleri sayfalama ve sıralama ile getirir (asenkron)
    public async Task<IQueryable<Product>> GetProductsByCategoryAsync(string category, int page, int pageSize, string sort = null)
    {
        try
        {
            // Ürünleri sorgulanabilir bir şekilde al
            var products = _context.Products
                .AsQueryable();

            // Eğer kategori belirtilmişse, ürünleri o kategoriye göre filtrele
            if (!string.IsNullOrEmpty(category))
            {
                var categoryUrl = category.ToLower(); // Kategori URL'sini küçük harfe çevir
                _logger.LogInformation($"Filtering by category URL: {categoryUrl}");

                // Kategoriye sahip ürünleri bul 
                products = products
                    .Where(p => p.Categories.Any(c => EF.Functions.Like(c.Url.ToLower(), categoryUrl)));
            }

            // Sıralama işlemi
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "price-asc": // Fiyata göre artan sıralama
                        products = products.OrderBy(p => p.Price);
                        break;
                    case "price-desc": // Fiyata göre azalan sıralama
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    default: // Varsayılan olarak ID'ye göre sırala
                        products = products.OrderBy(p => p.Id);
                        break;
                }
            }
            else
            {
                // Sıralama belirtilmemişse, ID'ye göre sırala
                products = products.OrderBy(p => p.Id);
            }

            // Sayfalama işlemi: Belirtilen sayfadaki ürünleri al
            var result = products
                .Skip((page - 1) * pageSize) // Önceki sayfaları atla
                .Take(pageSize); // Belirtilen sayıda ürün al

            // Bulunan ürünlerin ID'lerini logla
            var productIds = await result.Select(p => p.Id).ToListAsync();
            _logger.LogInformation($"Found products: {string.Join(", ", productIds)}");

            // Sonucu IQueryable olarak döndür
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while getting products for category: {category}, page: {page}");
            throw;
        }
    }

    // Belirtilen kategorideki ürün sayısını getirir senkron, henüz uygulanmadı.
    public int GetProductCount(string category)
    {
        throw new NotImplementedException();
    }
}