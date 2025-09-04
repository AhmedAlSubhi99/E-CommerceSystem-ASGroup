using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class OrderRepo : IOrderRepo
    {
        private readonly ApplicationDbContext _context;

        public OrderRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD ====================

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int oid)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OID == oid);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int uid)
        {
            return await _context.Orders
                .Where(o => o.UserId == uid)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task DeleteOrderAsync(int oid)
        {
            var order = await GetOrderByIdAsync(oid);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
        }

        // ==================== DETAILS ====================

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OID == orderId);
        }

        // ==================== UNIT OF WORK ====================

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
