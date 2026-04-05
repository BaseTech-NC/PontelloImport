using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PontelloImport.Models;

namespace PontelloImport.Services
{
    public interface IPdfService
    {
        byte[] GeneratePurchaseOrder(Order order, string? dealerEmail = null);
    }

    public class PdfService : IPdfService
    {
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
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PONTELLO IMPORTS")
                            .Bold().FontSize(18)
                            .FontColor("#0D3D38");
                        col.Item().Text("141 Eastchester Avenue")
                            .FontSize(9).FontColor("#6B7280");
                        col.Item().Text("St. Catharines, ON  L2P 2Z5")
                            .FontSize(9).FontColor("#6B7280");
                        col.Item().Text("647-964-6833")
                            .FontSize(9).FontColor("#6B7280");
                        col.Item().Text("jesse@pontelloimports.com")
                            .FontSize(9).FontColor("#6B7280");
                    });

                    row.ConstantItem(160).Column(col =>
                    {
                        col.Item().AlignRight()
                            .Text("PURCHASE ORDER")
                            .Bold().FontSize(20)
                            .FontColor("#028090");
                        col.Item().AlignRight()
                            .Text($"PO # {order.OrderNumber}")
                            .Bold().FontSize(12)
                            .FontColor("#111827");
                        col.Item().AlignRight()
                            .Text($"Date: {order.OrderDate:MMMM d, yyyy}")
                            .FontSize(9).FontColor("#6B7280");
                        col.Item().AlignRight()
                            .Text($"Due: {order.PaymentDueDate?.ToString("MMMM d, yyyy") ?? "TBD"}")
                            .FontSize(9).FontColor("#6B7280");
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

                    // Bill To / Ship To
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("BILL TO")
                                .Bold().FontSize(8)
                                .FontColor("#6B7280")
                                .LetterSpacing(1);
                            c.Item().Text(order.Dealer?.CompanyName ?? order.DealerCompanyName)
                                .Bold().FontSize(11);
                            if (!string.IsNullOrEmpty(dealerEmail))
                                c.Item().Text(dealerEmail).FontColor("#6B7280");
                            if (order.Dealer?.BillingAddress != null)
                            {
                                c.Item().Text(order.Dealer.BillingAddress.Street);
                                c.Item().Text(
                                    $"{order.Dealer.BillingAddress.City}, " +
                                    $"{order.Dealer.BillingAddress.Province} " +
                                    $"{order.Dealer.BillingAddress.PostalCode}");
                            }
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("SHIP TO")
                                .Bold().FontSize(8)
                                .FontColor("#6B7280")
                                .LetterSpacing(1);
                            var shipAddr = order.Dealer?.ShippingAddress
                                           ?? order.Dealer?.BillingAddress;
                            if (shipAddr != null)
                            {
                                c.Item().Text(order.Dealer?.CompanyName ?? "")
                                    .Bold().FontSize(11);
                                c.Item().Text(shipAddr.Street);
                                c.Item().Text(
                                    $"{shipAddr.City}, " +
                                    $"{shipAddr.Province} " +
                                    $"{shipAddr.PostalCode}");
                            }
                            else
                            {
                                c.Item().Text("Same as billing")
                                    .FontColor("#6B7280").FontSize(9);
                            }
                        });

                        row.ConstantItem(20);

                        row.ConstantItem(140).Column(c =>
                        {
                            c.Item().Text("ORDER INFO")
                                .Bold().FontSize(8)
                                .FontColor("#6B7280")
                                .LetterSpacing(1);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Payment Terms:")
                                    .FontColor("#6B7280");
                                r.RelativeItem().AlignRight()
                                    .Text(order.PaymentTerms?.TermName ?? "Net 30")
                                    .Bold();
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Tax Exempt:")
                                    .FontColor("#6B7280");
                                r.RelativeItem().AlignRight()
                                    .Text(order.IsTaxExempt ? "Yes" : "No")
                                    .Bold();
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Status:")
                                    .FontColor("#6B7280");
                                r.RelativeItem().AlignRight()
                                    .Text(order.Status)
                                    .Bold().FontColor("#028090");
                            });
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
                                if (!string.IsNullOrEmpty(line.VariantTitle) &&
                                    line.VariantTitle != "Default Title")
                                    dc.Item().Text(line.VariantTitle)
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
                                : "TBD");

                        totals.Item().PaddingVertical(4)
                            .LineHorizontal(1.5f).LineColor("#028090");

                        TotalRow("TOTAL DUE", $"${order.TotalAmount:F2}",
                            bold: true, color: "#0D3D38");
                    });

                    // Notes
                    col.Item().PaddingTop(8).Column(n =>
                    {
                        n.Item().Text("Notes").Bold().FontSize(9).FontColor("#6B7280");
                        n.Item().Text(
                            order.IsTaxExempt
                                ? "Tax exempt order — no HST applied."
                                : "HST applies to all Canadian orders.")
                            .FontSize(8).FontColor("#9CA3AF");
                        if (order.TrackingNumber != null)
                            n.Item().Text($"Tracking: {order.TrackingNumber}")
                                .FontSize(8).FontColor("#9CA3AF");
                    });
                });
            }

            void ComposeFooter(IContainer container)
            {
                container.Row(row =>
                {
                    row.RelativeItem().Text(
                        "Thank you for your business. Questions? 647-964-6833")
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
