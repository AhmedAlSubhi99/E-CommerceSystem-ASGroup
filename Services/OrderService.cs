using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;

namespace E_CommerceSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductService _productService;
        private readonly IOrderProductsService _orderProductsService;

        public OrderService(IOrderRepo orderRepo, IProductService productService, IOrderProductsService orderProductsService)
        {
            _orderRepo = orderRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
        }

        //get all orders for login user
        public List <OrderProducts> GetAllOrders(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid);
            if (orders == null || !orders.Any())
                throw new InvalidOperationException($"No orders found for user ID {uid}.");

            // Collect all OrderProducts for all orders
            var allOrderProducts = new List<OrderProducts>();

            foreach (var order in orders)
            {
                var orderProducts = _orderProductsService.GetOrdersByOrderId(order.OID);
                if (orderProducts != null)
                    allOrderProducts.AddRange(orderProducts);
            }

            return allOrderProducts;

        }

        //get order by order id for the login user
        public IEnumerable<OrdersOutputOTD> GetOrderById(int oid, int uid)
        {
            //list of items in the order 
            List<OrdersOutputOTD> items = new List<OrdersOutputOTD>();
            OrdersOutputOTD ordersOutputOTD = null;

            
            List<OrderProducts> products = null;
            Product product = null;
            string productName = string.Empty;

            //get order 
            var order = _orderRepo.GetOrderById(oid);

            if (order == null)
                throw new InvalidOperationException($"No orders found .");

            //execute the products data in existing Product
            if (order.UID == uid)
            {
                products = _orderProductsService.GetOrdersByOrderId(oid);
                foreach (var p in products)
                {
                    product = _productService.GetProductById(p.PID);
                    productName = product.ProductName;
                    ordersOutputOTD = new OrdersOutputOTD
                    {
                        ProductName = productName,
                        Quantity = p.Quantity,
                        OrderDate = order.OrderDate,
                        TotalAmount = p.Quantity * product.Price,
                    };
                    items.Add(ordersOutputOTD);
                }
            }
   
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
            throw new Exception($"order with ID {oid} is deleted");
        }
        public void AddOrder(Order order)
        {
            _orderRepo.AddOrder(order);
        }
        public void UpdateOrder(Order order)
        {
            _orderRepo.UpdateOrder(order);
        }

        //Places an order for the given list of items and user ID.
        public void PlaceOrder( List<OrderItemDTO> items, int uid)
        {
            // Temporary variable to hold the currently processed product
            Product existingProduct = null;
            
            decimal TotalPrice, totalOrderPrice = 0; // Variables to hold the total price of each item and the overall order

            OrderProducts orderProducts = null;

            // Validate all items in the order
            for (int i = 0; i < items.Count; i++)
            {
                TotalPrice = 0;
                existingProduct = _productService.GetProductByName(items[i].ProductName);
                if (existingProduct == null)
                    throw new Exception($"{items[i].ProductName} not Found");

                if (existingProduct.Stock < items[i].Quantity)
                    throw new Exception($"{items[i].ProductName} is out of stock");

            }
            // Create a new order for the user
            var order = new Order { UID = uid, OrderDate = DateTime.Now, TotalAmount = 0 };
            AddOrder(order); // Save the order to the database

            // Process each item in the order
            foreach (var item in items)
            {
                // Retrieve the product by its name
                existingProduct = _productService.GetProductByName(item.ProductName);
               
                // Calculate the total price for the current item
                TotalPrice = item.Quantity * existingProduct.Price;

                // Deduct the ordered quantity from the product's stock
                existingProduct.Stock -= item.Quantity;

                // Update the overall total order price
                totalOrderPrice += TotalPrice;

                // Create a relationship record between the order and product
                orderProducts = new OrderProducts {OID = order.OID, PID = existingProduct.PID, Quantity = item.Quantity  };
                _orderProductsService.AddOrderProducts(orderProducts);

                // Update the product's stock in the database
                _productService.UpdateProduct(existingProduct);
            }

            // Update the total amount of the order
            order.TotalAmount = totalOrderPrice;
            UpdateOrder(order);

        }
    }
}
