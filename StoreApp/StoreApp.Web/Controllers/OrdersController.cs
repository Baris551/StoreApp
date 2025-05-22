using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApp.Data.Concrete;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StoreApp.Web.Controllers
{
    [Authorize]
public class OrdersController : Controller
{
    private readonly StoreDbContext _context;

    public OrdersController(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            Console.WriteLine("UserId alınamadı, boş liste dönülüyor.");
            return View(new List<Order>());
        }
        Console.WriteLine($"Orders/Index - Kullanıcı UserId: {userId}");

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => User.IsInRole("Admin") ? true : (o.UserId == userId && o.OrderStatus == "Completed"))
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        Console.WriteLine($"Orders/Index - Çekilen sipariş sayısı: {orders.Count}");
        return View(orders);
    }
}
}