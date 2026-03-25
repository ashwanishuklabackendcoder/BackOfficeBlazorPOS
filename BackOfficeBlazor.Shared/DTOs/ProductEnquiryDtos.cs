using System;
using System.Collections.Generic;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ProductEnquiryHistoryDto
    {
        public int SuggestedStockLevel { get; set; }
        public List<ProductEnquiryHistoryRowDto> MonthlySummaries { get; set; } = new();
    }

    public class ProductEnquiryHistoryRowDto
    {
        public string Month { get; set; } = string.Empty;
        public int UnitsIn { get; set; }
        public int UnitsSold { get; set; }
        public decimal Turnover { get; set; }
        public decimal Profit { get; set; }
    }

    public class ProductEnquiryLocationDto
    {
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public int InStockQty { get; set; }
        public decimal StockValue { get; set; }
        public int ReturnedQty { get; set; }
        public int ReservedQty { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public int BackOrdersQty { get; set; }
        public int SalesLast12Months { get; set; }
    }

    public class ProductEnquiryTransferDto
    {
        public DateTime Date { get; set; }
        public int DispatchId { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string SalesCode { get; set; } = string.Empty;
    }

    public class ProductEnquiryPurchaseOrderDto
    {
        public string OrderNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Qty { get; set; }
        public int OutstandingQty { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CustomerAccount { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Supplier { get; set; } = string.Empty;
    }

    public class ProductEnquiryTransactionDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public int QtyIn { get; set; }
        public int QtyOut { get; set; }
        public string Location { get; set; } = string.Empty;
        public int StockBalanceAfter { get; set; }
        public string SalesPerson { get; set; } = string.Empty;
    }

    public class ProductEnquirySaleDto
    {
        public string Customer { get; set; } = string.Empty;
        public string Invoice { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public string Serial { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class ProductEnquiryStockCheckDto
    {
        public DateTime StockCheckDate { get; set; }
        public int SystemQty { get; set; }
        public int PhysicalQty { get; set; }
        public int Difference { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class ProductEnquiryLayawayDto
    {
        public string LayawayRef { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }

    public class ProductEnquiryLogDto
    {
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class ProductEnquiryInternalOrderDto
    {
        public string InternalOrderNo { get; set; } = string.Empty;
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
