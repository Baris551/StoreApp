using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StoreApp.Data.Abstract;
using System;

// Bu sınıf, siparişlerle ilgili veritabanı işlemlerini Entity Framework kullanarak gerçekleştirir.
namespace StoreApp.Data.Concrete
{
    public class EFOrderRepository : IOrderRepository
    {
        private readonly StoreDbContext _context;
        private readonly ILogger<EFOrderRepository> _logger;

        public EFOrderRepository(StoreDbContext context, ILogger<EFOrderRepository> logger)
        {
            _context = context; // Veritabanı bağlantisini başlatır
            _logger = logger;   // Loglama nesnesini başlatır
        }

        // Yeni bir sipariş oluşturur (asenkron)
        public async Task CreateOrderAsync(Order order)
        {
            try
            {
                // Siparişteki her bir ürün için stok kontrolü yap
                foreach (var item in order.OrderItems)
                {
                    // Ürünü veritabanından ID'sine göre bul
                    var product = await _context.Products.FindAsync(item.ProductId);
                    // Eğer ürün bulunamazsa veya stok yetersizse hata fırlat
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        // Yetersiz stok durumunnda uyarı logu yaz
                        _logger.LogWarning($"Insufficient stock for product ID {item.ProductId}. Required: {item.Quantity}, Available: {product?.StockQuantity ?? 0}");
                        // hata türkcesi
                        throw new InvalidOperationException($"Yetersiz stok: Ürün ID {item.ProductId} için yeterli stok bulunmamaktadır.");
                    }

                    // Stoğu güncelle: Sipariş edilen miktarı stoktan düş
                    product.StockQuantity -= item.Quantity;
                }

                // Siparişi veritabanına ekle
                _context.Orders.Add(order);
                // Değişiklikleri veritabanına kaydet 
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Order created with ID: {order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating order.");
                throw;
            }
        }

        // Belirtilen ID'ye sahip siparişi getirir (sipariş öğeleri ve ürünler dahil)
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            try
            {
                // Siparişi, sipariş öğelerini ve her bir öğenin ürününü veritabanından getir
                var order = await _context.Orders
                    .Include(o => o.OrderItems) // Sipariş öğelerini dahil et
                    .ThenInclude(oi => oi.Product) // Her bir sipariş öğesinin ürününü dahil et
                    .FirstOrDefaultAsync(o => o.Id == orderId); // ID'ye göre siparişi bul

                if (order == null)
                {
                    _logger.LogWarning($"Order with ID {orderId} not found.");
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while getting order with ID: {orderId}");
                throw;
            }
        }
    }
}