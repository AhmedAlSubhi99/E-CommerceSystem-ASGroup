using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace E_CommerceSystem.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IOrderService _orders;

        public InvoiceService(ApplicationDbContext ctx, IOrderService orders)
        {
            _ctx = ctx;
            _orders = orders;
        }

        public async Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int orderId, int requestUserId, bool isAdmin)
        {
            // 1) Load minimal order with owner for authorization
            var order = await _ctx.Orders
                .Include(o => o.user)
                .Include(o => o.OrderProducts).ThenInclude(i => i.product)
                .FirstOrDefaultAsync(o => o.OID == orderId);

            if (order == null)
                return null;

            // Only owner or admin
            if (!isAdmin && order.UID != requestUserId)
                return null;

            // 2) Use existing DTO builder (keeps logic consistent)
            var dto = await _orders.GetOrderDetails(orderId);
            if (dto == null)
                return null;

            QuestPDF.Settings.License = LicenseType.Community; // safety

            var fileName = $"invoice_{dto.OrderId}.pdf";
            var culture = CultureInfo.InvariantCulture;

            // 3) Build the PDF
            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("E-Commerce System").FontSize(20).SemiBold();
                                col.Item().Text($"Invoice #{dto.OrderId}").FontSize(12);
                                col.Item().Text($"Date: {dto.CreatedAt:yyyy-MM-dd HH:mm} UTC").FontSize(10);
                            });

                            row.ConstantItem(140).Column(col =>
                            {
                                col.Item().Text("BILL TO").SemiBold();
                                col.Item().Text(dto.CustomerName ?? "Customer");
                            });
                        });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // #
                                columns.RelativeColumn(6);   // Product
                                columns.RelativeColumn(2);   // Qty
                                columns.RelativeColumn(3);   // Unit
                                columns.RelativeColumn(3);   // Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(h => h.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Text("#").SemiBold();
                                header.Cell().Element(h => h.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Text("Product").SemiBold();
                                header.Cell().Element(h => h.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Text("Qty").SemiBold();
                                header.Cell().Element(h => h.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Text("Unit Price").SemiBold();
                                header.Cell().Element(h => h.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Text("Line Total").SemiBold();
                            });

                            int i = 1;
                            foreach (var line in dto.Lines)
                            {
                                table.Cell().PaddingVertical(4).Text(i.ToString());
                                table.Cell().PaddingVertical(4).Text(line.ProductName);
                                table.Cell().PaddingVertical(4).Text(line.Quantity.ToString(culture));
                                table.Cell().PaddingVertical(4).Text(line.UnitPrice.ToString("0.000", culture));
                                table.Cell().PaddingVertical(4).Text(line.LineTotal.ToString("0.000", culture));
                                i++;
                            }
                        });

                        col.Item().PaddingTop(12).Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(240).Column(sum =>
                            {
                                sum.Item().Row(r =>
                                {
                                    r.RelativeItem().AlignRight().Text("Subtotal:");
                                    r.ConstantItem(100).AlignRight().Text(dto.Subtotal.ToString("0.000", culture));
                                });

                                // Add VAT/discount rows here if needed

                                sum.Item().BorderTop(1).PaddingTop(6);
                                sum.Item().Row(r =>
                                {
                                    r.RelativeItem().AlignRight().Text("Total:").SemiBold();
                                    r.ConstantItem(100).AlignRight().Text(dto.Total.ToString("0.000", culture)).SemiBold();
                                });
                            });
                        });

                        col.Item().PaddingTop(20).Text("Thank you for your purchase!").Italic().FontSize(10);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ").FontSize(10);
                            x.CurrentPageNumber().FontSize(10);
                            x.Span(" of ").FontSize(10);
                            x.TotalPages().FontSize(10);
                        });
                });
            }).GeneratePdf();

            return (bytes, fileName);
        }
    }
}
