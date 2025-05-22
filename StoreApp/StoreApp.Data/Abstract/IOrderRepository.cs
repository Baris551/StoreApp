using StoreApp.Data.Concrete;

namespace StoreApp.Data.Abstract;

public interface IOrderRepository
{
    // IQueryable<Order> Orders { get; }// IQueryable<Order> Orders { get; } // veritabanındaki siparişleri listelemek için kullanıyoruz
    // void SaveOrder(Order order); // geri dönüş değeri olmayan metot veritabanına kaydediyoruz
    Task CreateOrderAsync(Order order); // Yeni metod: Sipariş oluşturma

    Task<Order> GetOrderByIdAsync(int orderId);
    
}
