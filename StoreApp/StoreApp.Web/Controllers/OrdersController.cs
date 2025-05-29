using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApp.Data.Concrete;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StoreApp.Web.Controllers
{
    // [Authorize] atributu: Bu controller'a sadece giriş yapmış (yetkili) kullanıcılar erişebilir.
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly StoreDbContext _context;

        public OrdersController(StoreDbContext context)
        {
            _context = context;
        }
            

        // Index metodu: Kullanıcının siparişlerini listeler (asenkron)
        public async Task<IActionResult> Index()
        {
            // Kullanıcının ID'sini all Kullanıcı giriş yaptığıngda, ClaimTypes.NameIdentifier ile kullanıcı ID'si alınır
            // int.TryParse: Kullanıcı ID'sini string'den sayıya çevirir, başarısız olursa false döner
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                // Eğer kullanıcı ID'si alınamazsa, konsola hata mesajı yaz ve boş bir sipariş listesi döndür
                Console.WriteLine("UserId alınamadı, boş liste dönülüyor.");
                return View(new List<Order>()); // Boş bir liste ile görünüm (view) döndürülür
            }

            Console.WriteLine($"Orders/Index - Kullanıcı UserId: {userId}");

            // Siparişleri veritabanından çek
            var orders = await _context.Orders
                .Include(o => o.OrderItems) // Siparişlerin içindeki ürünleri (OrderItems) de dahil et
                .Where(o => User.IsInRole("Admin") ? true : (o.UserId == userId && o.OrderStatus == "Completed")) // Filtreleme
                                                                                                                  // Eğer kullanıcı Admin ise tüm siparişleri göster, değilse sadece kendi tamamlanmış siparişlerini göster
                .OrderByDescending(o => o.OrderDate) // Siparişleri tarihe göre tersten sırala (en yeni önce)
                .ToListAsync(); // Verileri liste olarak getir (asenkron)

            Console.WriteLine($"Orders/Index - Çekilen sipariş sayısı: {orders.Count}");

            // Sipariş listesini görünüm (view) ile döndür
            return View(orders);
        }
    }
}