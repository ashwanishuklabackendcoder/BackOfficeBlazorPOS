using System;
using System.Collections.Generic;

namespace BackOfficeBlazor.Shared.DTOs
{
    public enum PurchaseOrderStatus
    {
        Draft = 0,
        Raised = 1,
        PartReceived = 2,
        Received = 3,
        Closed = 4,
        Cancelled = 9
    }

    public class PurchaseOrderLineDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int SequenceId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string MfrPartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BoxQty { get; set; }
        public int QtyRequired { get; set; }
        public decimal CostPrice { get; set; }
        public int QtyRecieved { get; set; }
        public string StockLocationCode { get; set; } = string.Empty;
        public string DeliveryLocationCode { get; set; } = string.Empty;
        public string CustomerAccNo { get; set; } = string.Empty;
        public bool IsMajor { get; set; }
        public bool XmasClub { get; set; }
        public bool SmsCustomerOnArrival { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string OrderedByCode { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public DateTime CreatedOnDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int InternalOrderRefID { get; set; }

        public decimal LineTotal => QtyRequired * CostPrice;
        public int OutstandingQty => Math.Max(QtyRequired - QtyRecieved, 0);
        public decimal ReceivedLineTotal => QtyRecieved * CostPrice;
    }

    public class PurchaseOrderHeaderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string RaisedByStaffCode { get; set; } = string.Empty;
        public DateTime RaisedOnDate { get; set; }
        public decimal CarriageCost { get; set; }
        public string? AmendedLastByCode { get; set; }
        public DateTime? AmendedLastOnDate { get; set; }
        public string? ClosedByCode { get; set; }
        public DateTime? ClosedOnDate { get; set; }
        public string? CancelledByCode { get; set; }
        public DateTime? CancelledOnDate { get; set; }
        public int Status { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public bool IsImported { get; set; }
        public string? JsonReport { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool DirectToStore { get; set; }
    }

    public class PurchaseOrderSummaryDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime RaisedOnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public int QtyOrdered { get; set; }
        public int QtyReceived { get; set; }
        public decimal CarriageCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class PurchaseOrderWorkspaceDto
    {
        public int DraftRef { get; set; }
        public PurchaseOrderHeaderDto? Header { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierAddress { get; set; } = string.Empty;
        public string? JsonReport { get; set; }
        public List<PurchaseOrderLineDto> Lines { get; set; } = new();
    }

    public class PurchaseOrderUpsertRequestDto
    {
        public int? DraftRef { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string RaisedByStaffCode { get; set; } = string.Empty;
        public decimal CarriageCost { get; set; }
        public bool DirectToStore { get; set; }
        public List<PurchaseOrderLineDto> Lines { get; set; } = new();
    }

    public class PurchaseOrderReceiveRequestDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string ReceivedByCode { get; set; } = string.Empty;
        public decimal CarriageCost { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal SettlementDiscount { get; set; }
        public bool DivideCarriageAcrossItems { get; set; } = true;
        public bool DivideShippingAcrossItems { get; set; } = true;
        public bool DivideSettlementDiscountAcrossItems { get; set; } = true;
        public bool UpdateProductItemPrices { get; set; }
        public List<PurchaseOrderReceiveLineDto> Lines { get; set; } = new();
    }

    public class PurchaseOrderReceiveLineDto
    {
        public int ItemId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public int SequenceId { get; set; }
        public int QtyReceived { get; set; }
        public decimal? ReceivedUnitCost { get; set; }
        public string? StockLocationCode { get; set; }
        public string? DeliveryLocationCode { get; set; }
    }

    public enum PurchaseOrderItemScope
    {
        All = 0,
        MajorOnly = 1,
        MinorOnly = 2
    }

    public class PurchaseOrderDirectRaiseRequestDto
    {
        public string LocationCode { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public string RaisedByStaffCode { get; set; } = string.Empty;
        public string SalesCode { get; set; } = string.Empty;
        public string FooterMessage { get; set; } = string.Empty;
        public PurchaseOrderItemScope ItemScope { get; set; } = PurchaseOrderItemScope.All;
        public bool PreviewOnly { get; set; } = true;
    }

    public class PurchaseOrderSearchDto
    {
        public string? Query { get; set; }
        public string? SupplierCode { get; set; }
        public int? Status { get; set; }
    }

    public class PurchaseOrderReportEnvelopeDto
    {
        public PurchaseOrderReportResultDto Result { get; set; } = new();
    }

    public class PurchaseOrderReportResultDto
    {
        public PurchaseOrderReportHeaderDto ReportHeader { get; set; } = new();
        public List<PurchaseOrderReportRowDto> Rows { get; set; } = new();
    }

    public class PurchaseOrderReportHeaderDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string WebAddress { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string SupplierAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string LocationNo { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string OurVatNumber { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string FooterMessage { get; set; } = string.Empty;
        public string SupplierEmail { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierAccNo { get; set; } = string.Empty;
        public string AbacusSupplierNo { get; set; } = string.Empty;
        public DateTime OrderDateObj { get; set; }
        public int Status { get; set; }
    }

    public class PurchaseOrderReportRowDto
    {
        public string YourPartNumber { get; set; } = string.Empty;
        public string OurPartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Multiplier { get; set; }
        public int BoxQty { get; set; }
        public int QuantityOrdered { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public string CustomerAcc { get; set; } = string.Empty;
        public string POType { get; set; } = "PO";
        public bool IsCancelled { get; set; }
    }
}
