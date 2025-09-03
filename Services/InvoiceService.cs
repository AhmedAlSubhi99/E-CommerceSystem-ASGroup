using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using E_CommerceSystem.DTOs;

namespace E_CommerceSystem.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IOrderService _orderService;

        public InvoiceService(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public byte[] GenerateInvoice(int orderId, int userId, string role)
        {
            // Get order details
            var order = _orderService.GetOrderSummary(orderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found.");

            // Security: Only owner or admin/manager can access
            if (order.UserId != userId && role != "admin" && role != "manager")
                throw new UnauthorizedAccessException("You are not allowed to view this invoice.");

            // Generate PDF
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Header
                    page.Header().Text($"Invoice #{order.OrderId}")
                        .FontSize(20).Bold().AlignCenter();

                    // Customer & Order Info
                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Customer: {order.CustomerName}").Bold();
                                c.Item().Text($"Email: {order.CustomerEmail}");
                                c.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd}");
                                c.Item().Text($"Status: {order.Status}");
                            });
                        });

                        // Products Table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(4); // Product
                                cols.RelativeColumn(2); // Quantity
                                cols.RelativeColumn(2); // Price
                                cols.RelativeColumn(2); // Subtotal
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Text("Product").Bold();
                                header.Cell().Text("Qty").Bold();
                                header.Cell().Text("Price").Bold();
                                header.Cell().Text("Subtotal").Bold();
                            });

                            // Table Rows
                            foreach (var line in order.Lines)
                            {
                                table.Cell().Text(line.ProductName);
                                table.Cell().Text(line.Quantity.ToString());
                                table.Cell().Text($"{line.Price:C}");
                                table.Cell().Text($"{line.Subtotal:C}");
                            }
                        });

                        // Total
                        col.Item().AlignRight().Text($"Total: {order.TotalAmount:C}")
                            .FontSize(14).Bold();
                    });

                    // Footer
                    page.Footer().AlignCenter().Text("Thank you for shopping with us!")
                        .FontSize(10).Italic();
                });
            }).GeneratePdf();

            return pdf;
        }
    }
}
