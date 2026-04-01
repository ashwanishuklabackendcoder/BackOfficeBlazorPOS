using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class SysOptionsDto
    {
        public int Id { get; set; }

        [StringLength(250)]
        public string Password1 { get; set; } = string.Empty;

        [StringLength(250)]
        public string Password2 { get; set; } = string.Empty;

        [StringLength(250)]
        public string Password3 { get; set; } = string.Empty;

        [StringLength(250)]
        public string PurchaseOrderMessage { get; set; } = string.Empty;
        public bool OrderRecievedIgnoreFrameEntry { get; set; }
        public bool SalesAnalysisXlsx { get; set; }
        public int StockTurnValue { get; set; }
        public decimal RoundDownWebRRPVal { get; set; }
        public bool RoundUpBoxQty { get; set; }
        public bool UpdateSupplierFromStockInput { get; set; }
        public bool UpdateWebsiteOrderReceived { get; set; }
        public bool UseTemplateMessages { get; set; }
        public bool POPrintSearch1 { get; set; }
        public bool POPrintSearch2 { get; set; }
        public bool POPrintDescription { get; set; }
        public bool POPrintSize { get; set; }
        public bool POPrintColor { get; set; }
        public bool POEmailOrderToSupplier { get; set; }
        public bool Layaway { get; set; }
        public bool KeywordProductSearch { get; set; }
        public int LayawayReservedForDays { get; set; }
        public bool LabelByUser { get; set; }
        public bool GenerateInvoiceOnCollection { get; set; }
        public bool GenerateInvoiceOnComplete { get; set; }
        public bool ZPrintCashBreakdown { get; set; }
        public bool ZPrintFloatBreakdown { get; set; }
        public bool ZPrintOtherTotals { get; set; }
        public bool ZPrintPaymentAnalysis { get; set; }
        public bool ZPrintSalesAnalysis { get; set; }
        public bool ZPrintVatAnalysis { get; set; }
        public bool ZShowTotalsOnEntry { get; set; }
        public bool BuildBikeComplete { get; set; }
        [StringLength(10)]
        public string CurrentYear { get; set; } = string.Empty;
        public bool QuickStockInput { get; set; }
        public bool UpdateCostOnInput { get; set; }
        public bool CreateModeIfMFRNotFound { get; set; }
        public bool UseLabelManager { get; set; }
        public bool CustomerOrderPrint { get; set; }
        public bool RaiseCustomerOrdersOnSave { get; set; }
        [StringLength(5)]
        public string TillPaymentCardId { get; set; } = string.Empty;

        [StringLength(5)]
        public string TillPaymentICardId { get; set; } = string.Empty;
        public bool POPrintNotes { get; set; }
        public bool CustomerOrderConfirmationOpenInMail { get; set; }
        public bool LabelPrintDescription { get; set; }
        [StringLength(250)]
        public string BACsDetails { get; set; } = string.Empty;
        public bool POAutoReorderByMin { get; set; }
        public bool POAutoReorderByMax { get; set; }
        public bool WebPrice99 { get; set; }
        public bool POAutoReorderLocWiseSplitEnabled { get; set; }
        public bool PrintPDIOnSale { get; set; }
        public bool WorkshopCustomPartsEnabled { get; set; }
        [StringLength(5)]
        public string WorkshopPart1 { get; set; } = string.Empty;

        [StringLength(100)]
        public string WorkshopPart1Name { get; set; } = string.Empty;

        [StringLength(5)]
        public string WorkshopPart2 { get; set; } = string.Empty;

        [StringLength(100)]
        public string WorkshopPart2Name { get; set; } = string.Empty;

        [StringLength(5)]
        public string WorkshopPart3 { get; set; } = string.Empty;

        [StringLength(100)]
        public string WorkshopPart3Name { get; set; } = string.Empty;

        [StringLength(5)]
        public string WorkshopPart4 { get; set; } = string.Empty;

        [StringLength(100)]
        public string WorkshopPart4Name { get; set; } = string.Empty;

        [StringLength(5)]
        public string WorkshopPart5 { get; set; } = string.Empty;

        [StringLength(100)]
        public string WorkshopPart5Name { get; set; } = string.Empty;
        public bool VeloLifeInsurance { get; set; }
        public bool BikmoInsurance { get; set; }
        public string WorkshopCustomerServiceJobCardNarrative { get; set; } = string.Empty;
        public bool LabelPrintPromoPrice { get; set; }
        [StringLength(100)]
        public string CompanyNo { get; set; } = string.Empty;

        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(500)]
        public string CompanyLogoUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string QuoteMessage { get; set; } = string.Empty;
        public bool AutoPrintServiceInv { get; set; }
        [StringLength(10)]
        public string NxtPartNo { get; set; } = string.Empty;

        [StringLength(10)]
        public string NxtDespNo { get; set; } = string.Empty;

        [StringLength(10)]
        public string NxtStockNo { get; set; } = string.Empty;

        [StringLength(10)]
        public string NxtOrderNo { get; set; } = string.Empty;

        [StringLength(10)]
        public string NxtOrderRefNo { get; set; } = string.Empty;

        [StringLength(10)]
        public string NxtInvoiceNo { get; set; } = string.Empty;
        public bool AutoInvoice { get; set; }
        public bool EnterPinOnTill { get; set; }
        public bool CreaditOnDayAcc { get; set; }
        [StringLength(10)]
        public string NxtBatchNo { get; set; } = string.Empty;

        [StringLength(10)]
        public string NxtQuoteNo { get; set; } = string.Empty;

        [StringLength(500)]
        public string TillMsg { get; set; } = string.Empty;

        [StringLength(500)]
        public string InvoiceMsg { get; set; } = string.Empty;

        [StringLength(500)]
        public string ServiceMessage { get; set; } = string.Empty;
        public bool AutoStockNo { get; set; }
        public bool MakeOnInvice { get; set; }
        public bool DefaultWebProd { get; set; }
        [StringLength(5)]
        public string WhareHouseShopNo { get; set; } = string.Empty;

        [StringLength(500)]
        public string BaseCurrency { get; set; } = string.Empty;
        public bool AllowSeparateComboItemReturn { get; set; } = true;
        [StringLength(30)]
        public string ComboPartialReturnRefundMode { get; set; } = "ALLOCATED";
        [StringLength(10)]
        public string DiscountPartNumber { get; set; } = string.Empty;
        [StringLength(40)]
        public string ReturnStockMode { get; set; } = ReturnStockModes.AskUserEveryReturn;
    }
}
