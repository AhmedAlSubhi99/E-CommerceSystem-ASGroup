using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductService _productService;
        private readonly IOrderProductsService _orderProductsService;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepo orderRepo,
            IProductService productService,
            IOrderProductsService orderProductsService,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _mapper = mapper;
        }

        // get all orders for logged-in user (kept as entities; you can add a DTO variant below)
        public List<OrderProducts> GetAllOrders(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid);
            if (orders == null || !orders.Any())
                throw new InvalidOperationException($"No orders found for user ID {uid}.");

            var allOrderProducts = new List<OrderProducts>();
            foreach (var order in orders)
            {
                // if you add a WithProduct variant, prefer it:
                var orderProducts = _orderProductsService.GetOrdersByOrderId(order.OID);
                if (orderProducts != null)
                    allOrderProducts.AddRange(orderProducts);
            }
            return allOrderProducts;
        }

        // convenience overload that returns DTOs
        public IEnumerable<OrdersOutputDTO> GetAllOrdersDto(int uid)
        {
            var entities = GetAllOrders(uid);
            return _mapper.Map<List<OrdersOutputDTO>>(entities);
        }

        // get order by order id for the login user -> now using AutoMapper
        public IEnumerable<OrdersOutputDTO> GetOrderById(int oid, int uid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null)
                throw new InvalidOperationException("No orders found.");

            if (order.UID != uid)
                return Enumerable.Empty<OrdersOutputDTO>();

            // Recommended: use the WithProduct method so Product & Order are loaded
            var lines = _orderProductsService.GetByOrderIdWithProduct(oid);

            // One-line mapping to your output DTO (computed fields come from MappingProfile)
            var items = _mapper.Map<List<OrdersOutputDTO>>(lines);
            return items;
        }

        public IEnumerable<Order> GetOrderByUserId(int uid)
        {
            var order = _orderRepo.GetOrderByUserId(uid);
            if (order == null)
                throw new KeyNotFoundException($"order with user ID {uid} not found.");
            return order;
        }

        public void DeleteOrder(int oid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null)
                throw new KeyNotFoundException($"order with ID {oid} not found.");

            _orderRepo.DeleteOrder(oid);

            // FIX: don't throw on success
            // consider returning void or a boolean
        }

        public void AddOrder(Order order) => _orderRepo.AddOrder(order);
        public void UpdateOrder(Order order) => _orderRepo.UpdateOrder(order);

        // Places an order (kept mostly as-is; mapping can't replace the lookups)
        public void PlaceOrder(List<OrderItemDTO> items, int uid)
        {
            Product existingProduct = null;
            decimal totalOrderPrice = 0;

            // Validate first
            foreach (var item in items)
            {
                existingProduct = _productService.GetProductByName(item.ProductName)
                    ?? throw new Exception($"{item.ProductName} not Found");

                if (existingProduct.Stock < item.Quantity)
                    throw new Exception($"{item.ProductName} is out of stock");
            }

            // Create order
            var order = new Order { UID = uid, OrderDate = DateTime.Now, TotalAmount = 0 };
            AddOrder(order);

            // Process lines
            foreach (var item in items)
            {
                existingProduct = _productService.GetProductByName(item.ProductName)!;

                var totalPrice = item.Quantity * existingProduct.Price;
                totalOrderPrice += totalPrice;

                existingProduct.Stock -= item.Quantity;

                var orderProducts = new OrderProducts
                {
                    OID = order.OID,
                    PID = existingProduct.PID,
                    Quantity = item.Quantity
                };

                _orderProductsService.AddOrderProducts(orderProducts);
                _productService.UpdateProduct(existingProduct);
            }

            order.TotalAmount = totalOrderPrice;
            UpdateOrder(order);
        }
    }
}
