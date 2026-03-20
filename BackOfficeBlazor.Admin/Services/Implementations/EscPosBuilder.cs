using System.Text;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;

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

        private static void AddText(List<byte> bytes, string text)
            => bytes.AddRange(Encoding.ASCII.GetBytes(text + "\r\n"));

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
