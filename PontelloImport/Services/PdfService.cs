using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PontelloImport.Models;
using Microsoft.AspNetCore.Hosting;

namespace PontelloImport.Services
{
    public interface IPdfService
    {
        byte[] GeneratePurchaseOrder(Order order, string? dealerEmail = null);
    }

    public class PdfService : IPdfService
    {
        private readonly IWebHostEnvironment _env;

        public PdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GeneratePurchaseOrder(Order order, string? dealerEmail = null)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x =>
                        x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ctx =>
                        ComposeContent(ctx, order, dealerEmail));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();

            void ComposeHeader(IContainer container)
            {
                container.Row(row =>
                {
                    // Left: logo + company info
                    row.RelativeItem().Column(col =>
                    {
                        var logoPath = Path.Combine(
                            _env.WebRootPath, "images", "pontello-Imports-Logo.png");
                        if (File.Exists(logoPath))
                        {
                            var logoBytes = File.ReadAllBytes(logoPath);
                            col.Item().Width(80).Image(logoBytes);
                            col.Item().Height(4);
                        }
                        col.Item().Text("PONTELLO IMPORTS")
                            .Bold().FontSize(14)
                            .FontColor("#0D3D38");
                        col.Item().Text("141 Eastchester Avenue")
                            .FontSize(8).FontColor("#6B7280");
                        col.Item().Text("St. Catharines, ON  L2P 2Z5")
                            .FontSize(8).FontColor("#6B7280");
                        col.Item().Text("647-964-6833  |  jesse@pontelloimports.com")
                            .FontSize(8).FontColor("#6B7280");
                    });

                    // Right: PO title block
                    row.ConstantItem(180).Column(col =>
                    {
                        col.Item().AlignCenter()
                            .Text("PURCHASE ORDER")
                            .Bold().FontSize(22)
                            .FontColor("#028090");
                        col.Item().Height(4);
                        col.Item().AlignCenter()
                            .Text($"# {order.PONumber}")
                            .Bold().FontSize(16)
                            .FontColor("#111827");
                        col.Item().Height(4);
                        col.Item().AlignCenter()
                            .Text($"Date: {order.OrderDate:MMMM d, yyyy}")
                            .FontSize(9).FontColor("#6B7280");
                        if (order.PaymentDueDate.HasValue)
                        {
                            col.Item().AlignCenter()
                                .Text($"Due: {order.PaymentDueDate.Value:MMMM d, yyyy}")
                                .FontSize(9).FontColor("#6B7280");
                        }
                    });
                });
            }

            void ComposeContent(IContainer container, Order order, string? dealerEmail)
            {
                container.Column(col =>
                {
                    col.Spacing(12);

                    // Divider
                    col.Item().PaddingVertical(4)
                        .LineHorizontal(1)
                        .LineColor("#028090");

                    // Determine if billing and shipping are effectively the same
                    var billAddr = order.Dealer?.BillingAddress;
                    var shipAddr = order.Dealer?.ShippingAddress;
                    bool sameAddress = shipAddr == null ||
                        (billAddr != null &&
                         string.Equals(shipAddr.Street, billAddr.Street, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(shipAddr.City, billAddr.City, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(shipAddr.PostalCode, billAddr.PostalCode, StringComparison.OrdinalIgnoreCase));

                    var displayAddr = sameAddress ? billAddr : null;

                    // Address + Order Details row
                    col.Item().Row(row =>
                    {
                        if (sameAddress)
                        {
                            // Single combined address block
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Bill to")
                                    .Bold().FontSize(10)
                                    .FontColor("#111827");
                                c.Item().PaddingBottom(4);
                                c.Item().Text(order.Dealer?.CompanyName ?? order.DealerCompanyName)
                                    .Bold().FontSize(11).FontColor("#111827");
                                if (!string.IsNullOrEmpty(dealerEmail))
                                    c.Item().Text(dealerEmail).FontSize(9).FontColor("#6B7280");
                                if (billAddr != null)
                                {
                                    c.Item().Text(billAddr.Street).FontSize(9).FontColor("#6B7280");
                                    c.Item().Text(
                                        $"{billAddr.City}, " +
                                        $"{billAddr.Province} " +
                                        $"{billAddr.PostalCode}").FontSize(9).FontColor("#6B7280");
                                    c.Item().Text(billAddr.Country ?? "Canada")
                                        .FontSize(9).FontColor("#6B7280");
                                }
                            });
                        }
                        else
                        {
                            // Separate Bill To
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Bill to")
                                    .Bold().FontSize(10)
                                    .FontColor("#111827");
                                c.Item().PaddingBottom(4);
                                c.Item().Text(order.Dealer?.CompanyName ?? order.DealerCompanyName)
                                    .Bold().FontSize(11).FontColor("#111827");
                                if (!string.IsNullOrEmpty(dealerEmail))
                                    c.Item().Text(dealerEmail).FontSize(9).FontColor("#6B7280");
                                if (billAddr != null)
                                {
                                    c.Item().Text(billAddr.Street).FontSize(9).FontColor("#6B7280");
                                    c.Item().Text(
                                        $"{billAddr.City}, " +
                                        $"{billAddr.Province} " +
                                        $"{billAddr.PostalCode}").FontSize(9).FontColor("#6B7280");
                                }
                            });

                            row.ConstantItem(20);

                            // Separate Ship To
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Ship to")
                                    .Bold().FontSize(10)
                                    .FontColor("#111827");
                                c.Item().PaddingBottom(4);
                                if (shipAddr != null)
                                {
                                    c.Item().Text(order.Dealer?.CompanyName ?? "")
                                        .Bold().FontSize(11).FontColor("#111827");
                                    c.Item().Text(shipAddr.Street).FontSize(9).FontColor("#6B7280");
                                    c.Item().Text(
                                        $"{shipAddr.City}, " +
                                        $"{shipAddr.Province} " +
                                        $"{shipAddr.PostalCode}").FontSize(9).FontColor("#6B7280");
                                }
                            });
                        }

                        row.ConstantItem(20);

                        // ORDER DETAILS — plain key:value rows, no cell backgrounds
                        row.ConstantItem(150).Column(c =>
                        {
                            c.Item().Text("ORDER DETAILS")
                                .Bold().FontSize(8)
                                .FontColor("#6B7280")
                                .LetterSpacing(1);
                            c.Item().Height(4);

                            void DetailRow(string label, string value, string? valueColor = null)
                            {
                                c.Item().BorderBottom(0.5f).BorderColor("#F1F5F9")
                                    .Row(r =>
                                    {
                                        r.RelativeItem().Padding(3)
                                            .Text(label).FontSize(8).FontColor("#6B7280");
                                        r.RelativeItem().Padding(3).AlignRight()
                                            .Text(value).Bold().FontSize(8)
                                            .FontColor(valueColor ?? "#111827");
                                    });
                            }

                            DetailRow("Payment Terms",
                                order.PaymentTerms?.TermName ?? "Net 30");
                            DetailRow("Tax Status",
                                order.IsTaxExempt ? "Tax Exempt" : "Taxable",
                                order.IsTaxExempt ? "#059669" : "#111827");
                            DetailRow("Order Status",
                                order.Status, "#028090");
                        });
                    });

                    // Line items table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(24);   // #
                            cols.RelativeColumn(4);    // Product
                            cols.RelativeColumn(2);    // SKU
                            cols.ConstantColumn(40);   // Qty
                            cols.RelativeColumn(1.5f); // Unit
                            cols.RelativeColumn(1.5f); // Total
                        });

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background("#0D3D38").Padding(6);

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("#")
                                .Bold().FontColor("#FFFFFF").FontSize(9);
                            h.Cell().Element(HeaderCell).Text("Description")
                                .Bold().FontColor("#FFFFFF").FontSize(9);
                            h.Cell().Element(HeaderCell).Text("SKU")
                                .Bold().FontColor("#FFFFFF").FontSize(9);
                            h.Cell().Element(HeaderCell).AlignCenter()
                                .Text("Qty").Bold().FontColor("#FFFFFF").FontSize(9);
                            h.Cell().Element(HeaderCell).AlignRight()
                                .Text("Unit Price").Bold().FontColor("#FFFFFF").FontSize(9);
                            h.Cell().Element(HeaderCell).AlignRight()
                                .Text("Line Total").Bold().FontColor("#FFFFFF").FontSize(9);
                        });

                        var lines = order.OrderLines.ToList();
                        for (int i = 0; i < lines.Count; i++)
                        {
                            var line = lines[i];
                            var bg = i % 2 == 0 ? "#FFFFFF" : "#F8FAFC";

                            IContainer DataCell(IContainer c) =>
                                c.Background(bg).Padding(6);

                            table.Cell().Element(DataCell)
                                .Text($"{i + 1}")
                                .FontColor("#6B7280").FontSize(9);

                            table.Cell().Element(DataCell).Column(dc =>
                            {
                                dc.Item().Text(line.ProductTitle).Bold();
                                var vt = line.VariantTitle;
                                if (!string.IsNullOrEmpty(vt) &&
                                    vt != "Default Title" && vt != "Title")
                                    dc.Item().Text(vt)
                                        .FontSize(8).FontColor("#6B7280");
                            });

                            table.Cell().Element(DataCell)
                                .Text(line.SKU ?? "")
                                .FontSize(8).FontColor("#6B7280");

                            table.Cell().Element(DataCell)
                                .AlignCenter().Text($"{line.Quantity}");

                            table.Cell().Element(DataCell)
                                .AlignRight().Text($"${line.UnitPrice:F2}");

                            table.Cell().Element(DataCell)
                                .AlignRight().Text($"${line.LineTotal:F2}").Bold();
                        }
                    });

                    // Totals
                    col.Item().AlignRight().Width(220).Column(totals =>
                    {
                        totals.Spacing(4);

                        void TotalRow(string label, string value,
                            bool bold = false, string? color = null)
                        {
                            totals.Item().Row(r =>
                            {
                                r.RelativeItem().Text(label)
                                    .FontColor("#6B7280");
                                var cell = r.ConstantItem(90).AlignRight();
                                if (bold)
                                    cell.Text(value).Bold()
                                        .FontColor(color ?? "#111827");
                                else
                                    cell.Text(value)
                                        .FontColor(color ?? "#111827");
                            });
                        }

                        var taxPct = order.TaxRate.HasValue
                            ? (order.TaxRate.Value * 100m).ToString("0.##")
                            : "13";

                        TotalRow("Subtotal", $"${order.SubtotalAmount:F2}");
                        TotalRow($"Tax (HST {taxPct}%)",
                            order.TaxAmount.HasValue ? $"${order.TaxAmount.Value:F2}" : "—");
                        TotalRow("Shipping",
                            order.ShippingCost.HasValue
                                ? $"${order.ShippingCost.Value:F2}"
                                : "Calculated after fulfillment");

                        totals.Item().PaddingVertical(4)
                            .LineHorizontal(1.5f).LineColor("#028090");

                        TotalRow("TOTAL DUE", $"${order.TotalAmount:F2}",
                            bold: true, color: "#0D3D38");
                    });

                    // Notes
                    col.Item().PaddingTop(8).Column(n =>
                    {
                        n.Item().Text("Notes").Bold().FontSize(9).FontColor("#6B7280");
                        n.Item().Height(2);
                        if (order.IsTaxExempt)
                        {
                            n.Item().Text("• Tax exempt order — no HST applied.")
                                .FontSize(8).FontColor("#9CA3AF");
                        }
                        else
                        {
                            n.Item().Text("• HST applies to all Canadian orders.")
                                .FontSize(8).FontColor("#9CA3AF");
                        }
                        n.Item().Text("• Shipping calculated after order processing.")
                            .FontSize(8).FontColor("#9CA3AF");
                        if (order.TrackingNumber != null)
                            n.Item().Text($"• Tracking: {order.TrackingNumber}")
                                .FontSize(8).FontColor("#9CA3AF");
                    });
                });
            }

            void ComposeFooter(IContainer container)
            {
                container.Row(row =>
                {
                    row.RelativeItem().Text(
                        "Thank you for your business.  Questions? 647-964-6833 | jesse@pontelloimports.com")
                        .FontSize(8).FontColor("#9CA3AF");
                    row.ConstantItem(60).AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor("#9CA3AF");
                        x.CurrentPageNumber().FontSize(8).FontColor("#9CA3AF");
                        x.Span(" of ").FontSize(8).FontColor("#9CA3AF");
                        x.TotalPages().FontSize(8).FontColor("#9CA3AF");
                    });
                });
            }
        }
    }
}
