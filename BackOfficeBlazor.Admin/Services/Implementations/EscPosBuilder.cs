using System.Text;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System.Net;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class EscPosBuilder : IEscPosBuilder
    {
        private const int ReceiptWidth = 32; // 58mm printer

        public string BuildTestReceipt()
        {
            var bytes = new List<byte>();
            bytes.AddRange(new byte[] { 0x1B, 0x40 }); // Initialize
            bytes.AddRange(Encoding.ASCII.GetBytes("CLOUD POS TEST RECEIPT\r\n"));
            bytes.AddRange(Encoding.ASCII.GetBytes(new string('-', ReceiptWidth) + "\r\n"));
            bytes.AddRange(Encoding.ASCII.GetBytes("Printer check successful.\r\n"));
            bytes.AddRange(Encoding.ASCII.GetBytes($"UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\r\n"));
            bytes.AddRange(Encoding.ASCII.GetBytes(new string('-', ReceiptWidth) + "\r\n"));
            bytes.AddRange(new byte[] { 0x1D, 0x56, 0x41, 0x03 }); // Partial cut
            return Convert.ToBase64String(bytes.ToArray());
        }

        public string BuildSaleReceipt(string invoiceNo, PosSaleRequestDto sale)
        {
            var bytes = new List<byte>();
            bytes.AddRange(new byte[] { 0x1B, 0x40 }); // Initialize

            AddCenter(bytes, "CLOUD POS");
            AddCenter(bytes, $"INVOICE {invoiceNo}");
            AddText(bytes, new string('-', ReceiptWidth));
            AddLabelValue(bytes, "Location", sale.Location);
            AddLabelValue(bytes, "Terminal", sale.Terminal);
            AddLabelValue(bytes, "Customer", sale.Customer);
            if (!string.IsNullOrWhiteSpace(sale.SalesPerson))
                AddLabelValue(bytes, "Sales Person", sale.SalesPerson);
            AddText(bytes, $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AddText(bytes, new string('-', ReceiptWidth));

            foreach (var line in sale.Lines) 
            {
                var itemCode = string.IsNullOrWhiteSpace(line.PartNumber) ? line.StockNo : line.PartNumber;
                itemCode = itemCode.Trim();

                AddWrapped(bytes, itemCode);
                if (!string.IsNullOrWhiteSpace(line.Description))
                    AddWrapped(bytes, line.Description);
                var qtyPrice = $"{line.Quantity}x{line.Sell:0.00}";
                var total = $"{line.Net:0.00}";
                AddText(bytes, FormatColumns(qtyPrice, total));

                if (line.IsCombo && line.ComboItems.Any())
                {
                    foreach (var child in line.ComboItems)
                    {
                        AddWrapped(bytes, $"  {child.ProductName} x {child.Qty}");
                        if (child.IsMajor && child.StockNumbers.Any())
                            AddWrapped(bytes, $"    Stock: {string.Join(", ", child.StockNumbers)}");
                    }
                }
            }

            AddText(bytes, new string('-', ReceiptWidth));
            AddText(bytes, FormatColumns("SubTotal", $"{sale.SubTotal:0.00}"));
            var totalDiscount = sale.Lines.Sum(x =>
            {
                var gross = x.Sell * x.Quantity;
                var discount = gross - x.Net;
                return discount > 0 ? discount : 0m;
            });
            if (totalDiscount > 0)
                AddText(bytes, FormatColumns("Discount", $"{totalDiscount:0.00}"));
            AddText(bytes, FormatColumns("VAT", $"{sale.VatAmount:0.00}"));
            AddText(bytes, FormatColumns("NetTotal", $"{sale.NetTotal:0.00}"));
            AddText(bytes, new string('-', ReceiptWidth));
            AddText(bytes, "Payment Details");
            if (sale.Payment.Cash > 0) AddText(bytes, FormatColumns("Cash", $"{sale.Payment.Cash:0.00}"));
            if (sale.Payment.Cheque > 0) AddText(bytes, FormatColumns("Cheque", $"{sale.Payment.Cheque:0.00}"));
            if (sale.Payment.MasterCard > 0) AddText(bytes, FormatColumns("MasterCard", $"{sale.Payment.MasterCard:0.00}"));
            if (sale.Payment.Visa > 0) AddText(bytes, FormatColumns("Visa", $"{sale.Payment.Visa:0.00}"));
            if (sale.Payment.Credit > 0) AddText(bytes, FormatColumns("Credit", $"{sale.Payment.Credit:0.00}"));
            AddText(bytes, FormatColumns("Total Paid", $"{sale.Payment.TotalTendered:0.00}"));

            var change = sale.Payment.Cash > sale.NetTotal
                ? sale.Payment.Cash - sale.NetTotal
                : 0m;
            AddText(bytes, FormatColumns("Change", $"{change:0.00}"));
            AddText(bytes, new string('-', ReceiptWidth));
            AddCenter(bytes, "THANK YOU");

            bytes.AddRange(new byte[] { 0x1D, 0x56, 0x41, 0x03 }); // Partial cut
            return Convert.ToBase64String(bytes.ToArray());
        }

        public string BuildSaleInvoiceHtml(string invoiceNo, PosSaleRequestDto sale, SysOptionsDto? sysOptions)
        {
            var companyName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(sysOptions?.CompanyName) ? "CLOUD POS" : sysOptions!.CompanyName);
            var logoUrl = WebUtility.HtmlEncode(sysOptions?.CompanyLogoUrl ?? string.Empty);
            var customer = WebUtility.HtmlEncode(sale.Customer ?? string.Empty);
            var terminal = WebUtility.HtmlEncode(sale.Terminal ?? string.Empty);
            var location = WebUtility.HtmlEncode(sale.Location ?? string.Empty);
            var salesPerson = WebUtility.HtmlEncode(sale.SalesPerson ?? string.Empty);

            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset=\"utf-8\" />");
            sb.Append("<style>");
            sb.Append("@page{size:A4 portrait;margin:10mm;}");
            sb.Append("body{font-family:Arial,sans-serif;color:#111;margin:0;font-size:10px;line-height:1.35;}");
            sb.Append(".page{padding:0 2mm;}");
            sb.Append(".brand-header{display:flex;align-items:center;justify-content:space-between;gap:12px;padding-bottom:10px;margin-bottom:12px;border-bottom:2px solid #d9e1ec;}");
            sb.Append(".brand-block{display:flex;align-items:center;gap:10px;min-width:0;}");
            sb.Append(".brand-logo{width:54px;height:54px;object-fit:contain;border-radius:8px;}");
            sb.Append(".brand-name{font-size:18px;font-weight:700;line-height:1.1;}");
            sb.Append(".doc-block{flex:1;text-align:center;}");
            sb.Append(".doc-title{font-size:18px;font-weight:800;letter-spacing:.08em;}");
            sb.Append(".doc-subtitle{font-size:10px;color:#667085;margin-top:2px;}");
            sb.Append(".info-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:10px 16px;margin-bottom:12px;}");
            sb.Append(".info-card{border:1px solid #e2e8f0;border-radius:8px;background:#f9fbfd;padding:10px 12px;}");
            sb.Append(".info-label{color:#667085;font-size:9px;text-transform:uppercase;letter-spacing:.06em;}");
            sb.Append(".info-value{margin-top:2px;font-size:11px;font-weight:600;}");
            sb.Append(".line-table{width:100%;border-collapse:collapse;table-layout:fixed;}");
            sb.Append(".line-table th,.line-table td{border:1px solid #d5dbe5;padding:6px 7px;vertical-align:top;}");
            sb.Append(".line-table th{background:#f5f7fa;font-size:9px;text-transform:uppercase;letter-spacing:.04em;}");
            sb.Append(".num{text-align:right;white-space:nowrap;}");
            sb.Append(".item-code{font-weight:600;white-space:pre-wrap;}");
            sb.Append(".muted{color:#667085;}");
            sb.Append(".small{font-size:9px;}");
            sb.Append(".totals{width:min(340px,100%);margin-left:auto;margin-top:14px;border:1px solid #d5dbe5;border-radius:8px;overflow:hidden;}");
            sb.Append(".totals-row{display:flex;justify-content:space-between;gap:8px;padding:7px 10px;border-bottom:1px solid #edf1f6;}");
            sb.Append(".totals-row:last-child{border-bottom:0;}");
            sb.Append(".totals-row.total{background:#f5f7fa;font-weight:700;}");
            sb.Append(".footer-note{margin-top:16px;padding-top:10px;border-top:1px solid #d9e1ec;text-align:center;}");
            sb.Append("</style></head><body><div class=\"page\">");
            sb.Append("<div class=\"brand-header\">");
            sb.Append("<div class=\"brand-block\">");
            if (!string.IsNullOrWhiteSpace(logoUrl))
                sb.Append($"<img src=\"{logoUrl}\" alt=\"Company logo\" class=\"brand-logo\" />");
            sb.Append($"<div class=\"brand-name\">{companyName}</div>");
            sb.Append("</div>");
            sb.Append("<div class=\"doc-block\">");
            sb.Append("<div class=\"doc-title\">INVOICE</div>");
            sb.Append($"<div class=\"doc-subtitle\">Invoice {WebUtility.HtmlEncode(invoiceNo)}</div>");
            sb.Append("</div>");
            sb.Append("<div style=\"width:54px;\"></div>");
            sb.Append("</div>");
            sb.Append("<div class=\"info-grid\">");
            AddInfoCard(sb, "Invoice No", invoiceNo);
            AddInfoCard(sb, "Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AddInfoCard(sb, "Location", location);
            AddInfoCard(sb, "Terminal", terminal);
            AddInfoCard(sb, "Customer", customer);
            AddInfoCard(sb, "Sales Person", string.IsNullOrWhiteSpace(salesPerson) ? "-" : salesPerson);
            sb.Append("</div>");

            sb.Append("<table class=\"line-table\"><thead><tr>");
            sb.Append("<th style=\"width:44%;\">Item</th><th style=\"width:12%;\" class=\"num\">Qty</th><th style=\"width:22%;\" class=\"num\">Unit</th><th style=\"width:22%;\" class=\"num\">Line Total</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var line in sale.Lines)
            {
                var itemCode = string.IsNullOrWhiteSpace(line.PartNumber) ? line.StockNo : line.PartNumber;
                itemCode = itemCode.Trim();
                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append($"<div class=\"item-code\">{WebUtility.HtmlEncode(itemCode)}</div>");
                if (!string.IsNullOrWhiteSpace(line.Description))
                    sb.Append($"<div class=\"muted small\">{WebUtility.HtmlEncode(line.Description)}</div>");
                if (line.IsCombo && line.ComboItems.Any())
                {
                    sb.Append("<div class=\"muted small\">");
                    foreach (var child in line.ComboItems)
                    {
                        sb.Append($"<div>{WebUtility.HtmlEncode(child.ProductName)} x {child.Qty}</div>");
                        if (child.IsMajor && child.StockNumbers.Any())
                            sb.Append($"<div>Stock: {WebUtility.HtmlEncode(string.Join(", ", child.StockNumbers))}</div>");
                    }
                    sb.Append("</div>");
                }
                sb.Append("</td>");
                sb.Append($"<td class=\"num\">{line.Quantity}</td>");
                sb.Append($"<td class=\"num\">{line.Sell:0.00}</td>");
                sb.Append($"<td class=\"num\">{line.Net:0.00}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            sb.Append("<div class=\"totals\">");
            AppendTotalRow(sb, "SubTotal", sale.SubTotal.ToString("0.00"));
            var totalDiscount = sale.Lines.Sum(x =>
            {
                var gross = x.Sell * x.Quantity;
                var discount = gross - x.Net;
                return discount > 0 ? discount : 0m;
            });
            if (totalDiscount > 0)
                AppendTotalRow(sb, "Discount", totalDiscount.ToString("0.00"));
            AppendTotalRow(sb, "VAT", sale.VatAmount.ToString("0.00"));
            AppendTotalRow(sb, "Net Total", sale.NetTotal.ToString("0.00"), true);
            AppendTotalRow(sb, "Total Paid", sale.Payment.TotalTendered.ToString("0.00"));
            var change = sale.Payment.Cash > sale.NetTotal ? sale.Payment.Cash - sale.NetTotal : 0m;
            AppendTotalRow(sb, "Change", change.ToString("0.00"), true);
            sb.Append("</div>");
            sb.Append("<div class=\"footer-note\">");
            if (!string.IsNullOrWhiteSpace(sysOptions?.TillMsg))
                sb.Append($"<div>{WebUtility.HtmlEncode(sysOptions.TillMsg)}</div>");
            sb.Append("<div class=\"mt-2 fw-semibold\">THANK YOU</div>");
            if (!string.IsNullOrWhiteSpace(sysOptions?.InvoiceMsg))
                sb.Append($"<div class=\"mt-1\">{WebUtility.HtmlEncode(sysOptions.InvoiceMsg)}</div>");
            sb.Append("</div>");
            sb.Append("</div></body></html>");

            return sb.ToString();
        }

        private static void AddText(List<byte> bytes, string text)
            => bytes.AddRange(Encoding.ASCII.GetBytes(text + "\r\n"));

        private static void AddInfoCard(StringBuilder sb, string label, string value)
        {
            sb.Append("<div class=\"info-card\">");
            sb.Append($"<div class=\"info-label\">{WebUtility.HtmlEncode(label)}</div>");
            sb.Append($"<div class=\"info-value\">{WebUtility.HtmlEncode(value)}</div>");
            sb.Append("</div>");
        }

        private static void AppendTotalRow(StringBuilder sb, string label, string value, bool total = false)
        {
            sb.Append(total ? "<div class=\"totals-row total\">" : "<div class=\"totals-row\">");
            sb.Append($"<span>{WebUtility.HtmlEncode(label)}</span><span>{WebUtility.HtmlEncode(value)}</span>");
            sb.Append("</div>");
        }

        private static void AddCenter(List<byte> bytes, string text)
        {
            bytes.AddRange(new byte[] { 0x1B, 0x61, 0x01 }); // Center
            bytes.AddRange(Encoding.ASCII.GetBytes(text + "\r\n"));
            bytes.AddRange(new byte[] { 0x1B, 0x61, 0x00 }); // Left
        }

        private static void AddLabelValue(List<byte> bytes, string label, string value)
        {
            var safeValue = value ?? "";
            AddWrapped(bytes, $"{label}: {safeValue}");
        }

        private static void AddWrapped(List<byte> bytes, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                AddText(bytes, "");
                return;
            }

            var remaining = text.Trim();
            while (remaining.Length > ReceiptWidth)
            {
                AddText(bytes, remaining[..ReceiptWidth]);
                remaining = remaining[ReceiptWidth..];
            }
            AddText(bytes, remaining);
        }

        private static string FormatColumns(string left, string right)
        {
            left ??= "";
            right ??= "";
            var maxLeft = ReceiptWidth - right.Length - 1;
            if (maxLeft < 1)
                return right.Length > ReceiptWidth ? right[..ReceiptWidth] : right.PadLeft(ReceiptWidth);

            if (left.Length > maxLeft)
                left = left[..maxLeft];

            return left.PadRight(maxLeft) + " " + right;
        }

    //    public string BuildSpecialOrderCancelTicketPayload()
    //    {
    //        int reasonType = 1;
    //        string reason = "test";
    //        string orderid = "12345";
    //        string accno = "A00001";
    //string partno = "P00001";
    //        string deliverTo = "01";
    //        string orderType = "test";
    //string soldOrViewingOrCyclescheme = "SOLD";


    //        bool warranty = false;
    //        bool workshop = false;
    //bool alreadyhere = false;
            
    //string notes = "";
    //        string date;
    //        string requiredByDate;
    //        string branch;
    //        var bytes = new List<byte>();

    //        // Initialize
    //        bytes.AddRange(new byte[] { 0x1B, 0x40 });

    //        string header = "";
    //        string footer = "";

    //        switch (reasonType)
    //        {
    //            case 1:
    //                header = "CANCELLED ORDER";
    //                footer = "CUSTOMER CANCELLED";
    //                reason = "";
    //                break;
    //            case 2:
    //                header = "PLEASE REQUEST FROM DIFFERENT LOCATION";
    //                footer = "CAN'T FIND PART";
    //                reason = "";
    //                break;
    //            case 3:
    //                header = "CHECK IF THIS NEEDS REQUESTED FROM ANOTHER LOCATION";
    //                footer = "OTHER";
    //                break;
    //        }

    //        // Center + Double Size + Bold
    //        bytes.AddRange(new byte[] { 0x1B, 0x61, 0x01 }); // Center
    //        bytes.AddRange(new byte[] { 0x1B, 0x21, 0x30 }); // Double height + width
    //        bytes.AddRange(Encoding.ASCII.GetBytes("*********************\r\n"));
    //        bytes.AddRange(Encoding.ASCII.GetBytes($"{header}\r\n\r\n"));

    //        // Normal text
    //        bytes.AddRange(new byte[] { 0x1B, 0x21, 0x00 }); // Normal
    //        bytes.AddRange(new byte[] { 0x1B, 0x61, 0x00 }); // Left

    //      //  bytes.AddRange(Encoding.ASCII.GetBytes($"REQUESTED DATE:{date}\r\n"));
    //     //   bytes.AddRange(Encoding.ASCII.GetBytes($"REQUESTED FROM:{branch}\r\n"));
    //        bytes.AddRange(Encoding.ASCII.GetBytes($"DELIVER TO:{deliverTo}\r\n"));
    //        bytes.AddRange(Encoding.ASCII.GetBytes($"ORDER TYPE:{orderType}\r\n"));
    //        bytes.AddRange(Encoding.ASCII.GetBytes($"T/S:{soldOrViewingOrCyclescheme}\r\n"));
    //       // bytes.AddRange(Encoding.ASCII.GetBytes($"REQUIRED BY DATE:{requiredByDate}\r\n\r\n"));

    //        void PrintFlag(string text)
    //        {
    //            bytes.AddRange(new byte[] { 0x1B, 0x21, 0x30 });
    //            bytes.AddRange(new byte[] { 0x1B, 0x61, 0x01 });
    //            bytes.AddRange(Encoding.ASCII.GetBytes($"{text}\r\n"));
    //            bytes.AddRange(new byte[] { 0x1B, 0x21, 0x00 });
    //            bytes.AddRange(new byte[] { 0x1B, 0x61, 0x00 });
    //        }

            

           

    //        if (workshop) PrintFlag("WORKSHOP");
    //        if (warranty) PrintFlag("WARRANTY");
    //        ;
    //        if (alreadyhere) PrintFlag("ALREADY HERE");

    //        // Barcode section
    //        void PrintBarcode(string value, string label)
    //        {
    //            bytes.AddRange(new byte[] { 0x1B, 0x61, 0x01 }); // Center
    //            bytes.AddRange(new byte[] { 0x1D, 0x68, 60 });   // Height
    //            bytes.AddRange(new byte[] { 0x1D, 0x77, 2 });    // Width

    //            var barcodeData = Encoding.ASCII.GetBytes(value);
    //            bytes.AddRange(new byte[] { 0x1D, 0x6B, 0x04 }); // CODE39
    //            bytes.AddRange(barcodeData);
    //            bytes.Add(0x00);

    //            bytes.AddRange(new byte[] { 0x1B, 0x61, 0x00 });
    //            bytes.AddRange(Encoding.ASCII.GetBytes($"{label}:{value}\r\n\r\n"));
    //        }

    //        PrintBarcode(accno, "ACCOUNT NO");
    //        PrintBarcode(partno, "PART NO");
    //        PrintBarcode(orderid, "ORDER ID");

    //        // Footer
    //        bytes.AddRange(new byte[] { 0x1B, 0x21, 0x30 });
    //        bytes.AddRange(new byte[] { 0x1B, 0x61, 0x01 });
    //        bytes.AddRange(Encoding.ASCII.GetBytes($"{footer}\r\n\r\n"));

    //        bytes.AddRange(new byte[] { 0x1B, 0x21, 0x00 });
    //        if (!string.IsNullOrEmpty(reason))
    //            bytes.AddRange(Encoding.ASCII.GetBytes($"{reason}\r\n"));

    //        bytes.AddRange(Encoding.ASCII.GetBytes($"{notes}\r\n\r\n"));
    //        bytes.AddRange(Encoding.ASCII.GetBytes("*********************\r\n\r\n\r\n"));

    //        // Cut paper
    //        bytes.AddRange(new byte[] { 0x1D, 0x56, 0x41, 0x03 });

    //        return Convert.ToBase64String(bytes.ToArray());
    //    }
    }
}
