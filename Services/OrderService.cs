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

        // Fetches ALL order lines (OrderProducts) for a given user
        public List<OrderProducts> GetAllOrders(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid) ?? Enumerable.Empty<Order>();
            var allOrderProducts = new List<OrderProducts>();

            foreach (var order in orders)
            {
                var lines = _orderProductsService.GetOrdersByOrderId(order.OID);
                if (lines != null) allOrderProducts.AddRange(lines);
            }

            return allOrderProducts;
        }


        public IEnumerable<Order> GetAllOrders()
        {
            return _orderRepo.GetAllOrders(); // repo should return IEnumerable<Order>
        }


        public IEnumerable<OrdersOutputDTO> GetAllOrdersDto(int uid)
        {
            var lines = GetAllOrders(uid); // List<OrderProducts>
            return _mapper.Map<List<OrdersOutputDTO>>(lines);
        }

        public IEnumerable<OrdersOutputDTO> GetOrderById(int oid, int uid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null || order.UID != uid)
                return Enumerable.Empty<OrdersOutputDTO>();

            // Prefer the repo/service that loads Product with the line if you have it
            var linesWithProducts = _orderProductsService.GetByOrderIdWithProduct(oid)
                                   ?? _orderProductsService.GetOrdersByOrderId(oid)
                                   ?? Enumerable.Empty<OrderProducts>();

            return _mapper.Map<List<OrdersOutputDTO>>(linesWithProducts);
        }

        public IEnumerable<Order> GetOrderByUserId(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid);
            if (orders == null)
                throw new KeyNotFoundException($"Orders for user {uid} not found.");
            return orders;
        }

        public Order? GetOrderEntityById(int oid) => _orderRepo.GetOrderById(oid);

        public void DeleteOrder(int oid)
        {
            var order = _orderRepo.GetOrderById(oid)
                        ?? throw new KeyNotFoundException($"Order {oid} not found.");
            _orderRepo.DeleteOrder(oid);
        }

        public void AddOrder(Order order) => _orderRepo.AddOrder(order);
        public void UpdateOrder(Order order) => _orderRepo.UpdateOrder(order);

        public void PlaceOrder(List<OrderItemDTO> items, int uid)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Order items cannot be empty.", nameof(items));

            decimal totalOrderPrice = 0m;

            // 1) Validate stock
            foreach (var item in items)
            {
                var product = _productService.GetProductByName(item.ProductName)
                              ?? throw new Exception($"{item.ProductName} not found");
                if (product.Stock < item.Quantity)
                    throw new Exception($"{item.ProductName} is out of stock");
            }

            // 2) Create order shell
            var order = new Order { UID = uid, OrderDate = DateTime.Now, TotalAmount = 0m };
            AddOrder(order);

            // 3) Add lines + update stock
            foreach (var item in items)
            {
                var product = _productService.GetProductByName(item.ProductName)!;

                var lineTotal = item.Quantity * product.Price;
                totalOrderPrice += lineTotal;

                product.Stock -= item.Quantity;

                var orderProducts = new OrderProducts
                {
                    OID = order.OID,
                    PID = product.PID,
                    Quantity = item.Quantity
                };

                _orderProductsService.AddOrderProducts(orderProducts);
                _productService.UpdateProduct(product);
            }

            // 4) Finalize order total
            order.TotalAmount = totalOrderPrice;
            UpdateOrder(order);
        }
    }
}
