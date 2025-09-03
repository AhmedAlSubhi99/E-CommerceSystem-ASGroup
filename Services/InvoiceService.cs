using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IOrderSummaryService _orderSummaryService;

        public InvoiceService(IOrderSummaryService orderSummaryService)
        {
            _orderSummaryService = orderSummaryService;
        }

        // ---------------------------
        // Sync implementation
        // ---------------------------
        public byte[]? GenerateInvoice(int orderId, int requestUserId, bool isAdmin)
        {
            var order = _orderSummaryService.GetOrderSummary(orderId);
            if (order == null) return null;

            if (order.UID != requestUserId && !isAdmin)
                return null;

            return BuildPdf(order);
        }

        // ---------------------------
        // Async implementation
        // ---------------------------
        public async Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int orderId, int requestUserId)
        {
            var order = await _orderSummaryService.GetOrderSummaryAsync(orderId);
            if (order == null) return null;

            if (order.UID != requestUserId && order.Role != "admin")
                return null;

            var pdfBytes = BuildPdf(order);
            var fileName = $"Invoice_{orderId}.pdf";
            return (pdfBytes, fileName);
        }

        // ---------------------------
        // Common PDF builder
        // ---------------------------
        private byte[] BuildPdf(OrderSummaryDTO order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header()
                        .Text($"Invoice #{order.OrderId}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Customer: {order.CustomerName}").FontSize(12);
                        col.Item().Text($"Date: {order.OrderDate:yyyy-MM-dd}").FontSize(12);
                        col.Item().LineHorizontal(1);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Product").Bold();
                                header.Cell().Text("Qty").Bold();
                                header.Cell().Text("Price").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            foreach (var item in order.Lines)
                            {
                                table.Cell().Text(item.ProductName);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"${item.UnitPrice:F2}");
                                table.Cell().Text($"${item.LineTotal:F2}");
                            }
                        });

                        col.Item().LineHorizontal(1);

                        col.Item().AlignRight()
                            .Text($"Total: ${order.TotalAmount:F2}")
                            .Bold().FontSize(14);
                    });

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
