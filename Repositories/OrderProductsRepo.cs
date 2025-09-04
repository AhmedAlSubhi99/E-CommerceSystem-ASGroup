using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class OrderProductsRepo : IOrderProductsRepo
    {
        private readonly ApplicationDbContext _context;

        public OrderProductsRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CREATE ====================
        public async Task AddOrderProductsAsync(OrderProducts orderProduct)
        {
            await _context.OrderProducts.AddAsync(orderProduct);
        }

        // ==================== READ ====================
        public async Task<IEnumerable<OrderProducts>> GetAllOrdersAsync()
        {
            return await _context.OrderProducts
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderProducts>> GetByOrderIdAsync(int orderId, bool includeProduct = false)
        {
            var query = _context.OrderProducts.AsQueryable();

            if (includeProduct)
            {
                query = query
                    .Include(op => op.Product)
                    .Include(op => op.Order);
            }

            return await query
                .Where(op => op.OID == orderId)
                .ToListAsync();
        }

        // ==================== UNIT OF WORK ====================
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
