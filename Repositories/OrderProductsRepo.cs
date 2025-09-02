using E_CommerceSystem.Models;
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

        public void AddOrderProducts(OrderProducts product)
        {
            try
            {
                _context.OrderProducts.Add(product);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public IEnumerable<OrderProducts> GetAllOrders()
        {
            try
            {
                return _context.OrderProducts.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
        public List<OrderProducts> GetOrdersByOrderId(int oid)
        {
            try
            {
                return _context.OrderProducts.Where(p => p.OID == oid).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
        public IList<OrderProducts> GetByOrderIdWithProduct(int orderId)
        {
            return _context.OrderProducts
                .Include(op => op.product)
                .Include(op => op.Order)
                .Where(op => op.OID == orderId)
                .ToList();
        }
    }
}
