using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StoreApp.Data.Abstract;
using StoreApp.Data.Concrete;

namespace StoreApp.Data.Concrete
{
    public class EFOrderRepository : IOrderRepository
    {
        private readonly StoreDbContext _context;

        public EFOrderRepository(StoreDbContext context)
        {
            _context = context;
        }

        public async Task CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems) // Sipariş öğelerini yükle
                .ThenInclude(oi => oi.Product) // Her sipariş öğesinin ürününü yükle
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}