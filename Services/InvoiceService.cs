using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IOrderSummaryService orderSummaryService, ILogger<InvoiceService> logger)
        {
            _orderSummaryService = orderSummaryService;
            _logger = logger;
        }

        // ---------------------------
        // Sync implementation
        // ---------------------------
        public byte[]? GenerateInvoice(int orderId, int requestUserId, bool isAdmin)
        {
            try
            {
                var order = _orderSummaryService.GetOrderSummary(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Invoice generation failed: Order {OrderId} not found.", orderId);
                    return null;
                }

                if (order.UID != requestUserId && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized invoice access attempt for Order {OrderId} by User {UserId}.", orderId, requestUserId);
                    return null;
                }

                _logger.LogInformation("Invoice generated successfully for Order {OrderId} by User {UserId}.", orderId, requestUserId);
                return BuildPdf(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for Order {OrderId}.", orderId);
                throw;
            }
        }

        // ---------------------------
        // Async implementation
        // ---------------------------
        public async Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int orderId, int requestUserId)
        {
            try
            {
                var order = await _orderSummaryService.GetOrderSummaryAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Async invoice generation failed: Order {OrderId} not found.", orderId);
                    return null;
                }

                if (order.UID != requestUserId && order.Role != "admin")
                {
                    _logger.LogWarning("Unauthorized async invoice access attempt for Order {OrderId} by User {UserId}.", orderId, requestUserId);
                    return null;
                }

                var pdfBytes = BuildPdf(order);
                var fileName = $"Invoice_{orderId}.pdf";

                _logger.LogInformation("Async invoice generated for Order {OrderId} by User {UserId}. File: {FileName}",
                                       orderId, requestUserId, fileName);

                return (pdfBytes, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating async invoice for Order {OrderId}.", orderId);
                throw;
            }
        }

        // ---------------------------
        // Common PDF builder
        // ---------------------------
        private byte[] BuildPdf(OrderSummaryDTO order)
        {
            _logger.LogDebug("Building PDF for Order {OrderId} with {LineCount} lines.", order.OrderId, order.Lines.Count);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    // Header
                    page.Header()
                        .Text($"Invoice #{order.OrderId}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    // Content
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Customer: {order.CustomerName}").FontSize(12);
                        col.Item().Text($"Date: {order.OrderDate:yyyy-MM-dd}").FontSize(12);
                        col.Item().LineHorizontal(1);

                        // Product Table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Text("Product").Bold();
                                header.Cell().Text("Qty").Bold();
                                header.Cell().Text("Price").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            // Data rows
                            foreach (var item in order.Lines)
                            {
                                table.Cell().Text(item.ProductName);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"${item.UnitPrice:F2}");
                                table.Cell().Text($"${item.LineTotal:F2}");
                            }
                        });

                        col.Item().LineHorizontal(1);

                        // Total
                        col.Item().AlignRight()
                            .Text($"Total: ${order.TotalAmount:F2}")
                            .Bold().FontSize(14);
                    });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for your purchase!")
                        .Italic().FontSize(10);
                });
            });

            return document.GeneratePdf();
        }
    }
}
