using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class SysOptionService : ISysOptionService
    {
        private readonly ISysOptionRepository _repo;

        public SysOptionService(ISysOptionRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<SysOptionsDto>> GetAsync()
        {
            try
            {
                var entity = await _repo.GetAsync();
                if (entity == null)
                {
                    entity = CreateDefaults();
                    await _repo.AddAsync(entity);
                    await _repo.SaveChangesAsync();
                }

                return ApiResponse<SysOptionsDto>.Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                return ApiResponse<SysOptionsDto>.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse<SysOptionsDto>> SaveAsync(SysOptionsDto dto)
        {
            try
            {
                var entity = await _repo.GetAsync();
                if (entity == null)
                {
                    entity = CreateDefaults();
                    await _repo.AddAsync(entity);
                }

                Apply(dto, entity);
                await _repo.SaveChangesAsync();

                return ApiResponse<SysOptionsDto>.Ok(ToDto(entity), "System settings saved.");
            }
            catch (Exception ex)
            {
                return ApiResponse<SysOptionsDto>.Fail($"Save failed: {ex.Message}");
            }
        }

        private static SysOption CreateDefaults()
        {
            return new SysOption
            {
                Password1 = string.Empty,
                Password2 = string.Empty,
                Password3 = string.Empty,
                PurchaseOrderMessage = string.Empty,
                OrderRecievedIgnoreFrameEntry = false,
                SalesAnalysisXlsx = false,
                StockTurnValue = 0,
                RoundDownWebRRPVal = 0m,
                RoundUpBoxQty = false,
                UpdateSupplierFromStockInput = false,
                UpdateWebsiteOrderReceived = false,
                UseTemplateMessages = false,
                POPrintSearch1 = false,
                POPrintSearch2 = false,
                POPrintDescription = false,
                POPrintSize = false,
                POPrintColor = false,
                POEmailOrderToSupplier = false,
                Layaway = false,
                KeywordProductSearch = false,
                LayawayReservedForDays = 0,
                LabelByUser = false,
                GenerateInvoiceOnCollection = false,
                GenerateInvoiceOnComplete = false,
                ZPrintCashBreakdown = false,
                ZPrintFloatBreakdown = false,
                ZPrintOtherTotals = false,
                ZPrintPaymentAnalysis = false,
                ZPrintSalesAnalysis = false,
                ZPrintVatAnalysis = false,
                ZShowTotalsOnEntry = false,
                BuildBikeComplete = false,
                CurrentYear = string.Empty,
                QuickStockInput = false,
                UpdateCostOnInput = false,
                CreateModeIfMFRNotFound = false,
                UseLabelManager = false,
                CustomerOrderPrint = false,
                RaiseCustomerOrdersOnSave = false,
                TillPaymentCardId = string.Empty,
                TillPaymentICardId = string.Empty,
                POPrintNotes = false,
                CustomerOrderConfirmationOpenInMail = false,
                LabelPrintDescription = false,
                BACsDetails = string.Empty,
                POAutoReorderByMin = false,
                POAutoReorderByMax = true,
                WebPrice99 = false,
                POAutoReorderLocWiseSplitEnabled = false,
                PrintPDIOnSale = false,
                WorkshopCustomPartsEnabled = false,
                WorkshopPart1 = string.Empty,
                WorkshopPart1Name = string.Empty,
                WorkshopPart2 = string.Empty,
                WorkshopPart2Name = string.Empty,
                WorkshopPart3 = string.Empty,
                WorkshopPart3Name = string.Empty,
                WorkshopPart4 = string.Empty,
                WorkshopPart4Name = string.Empty,
                WorkshopPart5 = string.Empty,
                WorkshopPart5Name = string.Empty,
                VeloLifeInsurance = false,
                BikmoInsurance = false,
                WorkshopCustomerServiceJobCardNarrative = string.Empty,
                LabelPrintPromoPrice = false,
                CompanyNo = string.Empty,
                CompanyName = string.Empty,
                CompanyLogoUrl = string.Empty,
                QuoteMessage = string.Empty,
                AutoPrintServiceInv = false,
                NxtPartNo = string.Empty,
                NxtDespNo = string.Empty,
                NxtStockNo = string.Empty,
                NxtOrderNo = string.Empty,
                NxtOrderRefNo = string.Empty,
                NxtInvoiceNo = string.Empty,
                AutoInvoice = false,
                EnterPinOnTill = false,
                CreaditOnDayAcc = false,
                NxtBatchNo = string.Empty,
                NxtQuoteNo = string.Empty,
                TillMsg = string.Empty,
                InvoiceMsg = string.Empty,
                ServiceMessage = string.Empty,
                AutoStockNo = false,
                MakeOnInvice = false,
                DefaultWebProd = false,
                WhareHouseShopNo = string.Empty,
                BaseCurrency = string.Empty,
                AllowSeparateComboItemReturn = true,
                ComboPartialReturnRefundMode = "ALLOCATED",
                DiscountPartNumber = string.Empty,
                ReturnStockMode = ReturnStockModes.AskUserEveryReturn
            };
        }

        private static SysOptionsDto ToDto(SysOption x)
        {
            return new SysOptionsDto
            {
                Id = x.Id,
                Password1 = x.Password1 ?? string.Empty,
                Password2 = x.Password2 ?? string.Empty,
                Password3 = x.Password3 ?? string.Empty,
                PurchaseOrderMessage = x.PurchaseOrderMessage ?? string.Empty,
                OrderRecievedIgnoreFrameEntry = x.OrderRecievedIgnoreFrameEntry ?? false,
                SalesAnalysisXlsx = x.SalesAnalysisXlsx ?? false,
                StockTurnValue = x.StockTurnValue ?? 0,
                RoundDownWebRRPVal = x.RoundDownWebRRPVal ?? 0m,
                RoundUpBoxQty = x.RoundUpBoxQty ?? false,
                UpdateSupplierFromStockInput = x.UpdateSupplierFromStockInput ?? false,
                UpdateWebsiteOrderReceived = x.UpdateWebsiteOrderReceived ?? false,
                UseTemplateMessages = x.UseTemplateMessages ?? false,
                POPrintSearch1 = x.POPrintSearch1 ?? false,
                POPrintSearch2 = x.POPrintSearch2 ?? false,
                POPrintDescription = x.POPrintDescription ?? false,
                POPrintSize = x.POPrintSize ?? false,
                POPrintColor = x.POPrintColor ?? false,
                POEmailOrderToSupplier = x.POEmailOrderToSupplier ?? false,
                Layaway = x.Layaway ?? false,
                KeywordProductSearch = x.KeywordProductSearch ?? false,
                LayawayReservedForDays = x.LayawayReservedForDays ?? 0,
                LabelByUser = x.LabelByUser ?? false,
                GenerateInvoiceOnCollection = x.GenerateInvoiceOnCollection ?? false,
                GenerateInvoiceOnComplete = x.GenerateInvoiceOnComplete ?? false,
                ZPrintCashBreakdown = x.ZPrintCashBreakdown ?? false,
                ZPrintFloatBreakdown = x.ZPrintFloatBreakdown ?? false,
                ZPrintOtherTotals = x.ZPrintOtherTotals ?? false,
                ZPrintPaymentAnalysis = x.ZPrintPaymentAnalysis ?? false,
                ZPrintSalesAnalysis = x.ZPrintSalesAnalysis ?? false,
                ZPrintVatAnalysis = x.ZPrintVatAnalysis ?? false,
                ZShowTotalsOnEntry = x.ZShowTotalsOnEntry ?? false,
                BuildBikeComplete = x.BuildBikeComplete ?? false,
                CurrentYear = x.CurrentYear ?? string.Empty,
                QuickStockInput = x.QuickStockInput ?? false,
                UpdateCostOnInput = x.UpdateCostOnInput ?? false,
                CreateModeIfMFRNotFound = x.CreateModeIfMFRNotFound ?? false,
                UseLabelManager = x.UseLabelManager ?? false,
                CustomerOrderPrint = x.CustomerOrderPrint ?? false,
                RaiseCustomerOrdersOnSave = x.RaiseCustomerOrdersOnSave ?? false,
                TillPaymentCardId = x.TillPaymentCardId ?? string.Empty,
                TillPaymentICardId = x.TillPaymentICardId ?? string.Empty,
                POPrintNotes = x.POPrintNotes ?? false,
                CustomerOrderConfirmationOpenInMail = x.CustomerOrderConfirmationOpenInMail ?? false,
                LabelPrintDescription = x.LabelPrintDescription ?? false,
                BACsDetails = x.BACsDetails ?? string.Empty,
                POAutoReorderByMin = x.POAutoReorderByMin ?? false,
                POAutoReorderByMax = x.POAutoReorderByMax ?? true,
                WebPrice99 = x.WebPrice99 ?? false,
                POAutoReorderLocWiseSplitEnabled = x.POAutoReorderLocWiseSplitEnabled ?? false,
                PrintPDIOnSale = x.PrintPDIOnSale ?? false,
                WorkshopCustomPartsEnabled = x.WorkshopCustomPartsEnabled ?? false,
                WorkshopPart1 = x.WorkshopPart1 ?? string.Empty,
                WorkshopPart1Name = x.WorkshopPart1Name ?? string.Empty,
                WorkshopPart2 = x.WorkshopPart2 ?? string.Empty,
                WorkshopPart2Name = x.WorkshopPart2Name ?? string.Empty,
                WorkshopPart3 = x.WorkshopPart3 ?? string.Empty,
                WorkshopPart3Name = x.WorkshopPart3Name ?? string.Empty,
                WorkshopPart4 = x.WorkshopPart4 ?? string.Empty,
                WorkshopPart4Name = x.WorkshopPart4Name ?? string.Empty,
                WorkshopPart5 = x.WorkshopPart5 ?? string.Empty,
                WorkshopPart5Name = x.WorkshopPart5Name ?? string.Empty,
                VeloLifeInsurance = x.VeloLifeInsurance ?? false,
                BikmoInsurance = x.BikmoInsurance ?? false,
                WorkshopCustomerServiceJobCardNarrative = x.WorkshopCustomerServiceJobCardNarrative ?? string.Empty,
                LabelPrintPromoPrice = x.LabelPrintPromoPrice ?? false,
                CompanyNo = x.CompanyNo ?? string.Empty,
                CompanyName = x.CompanyName ?? string.Empty,
                CompanyLogoUrl = x.CompanyLogoUrl ?? string.Empty,
                QuoteMessage = x.QuoteMessage ?? string.Empty,
                AutoPrintServiceInv = x.AutoPrintServiceInv ?? false,
                NxtPartNo = x.NxtPartNo ?? string.Empty,
                NxtDespNo = x.NxtDespNo ?? string.Empty,
                NxtStockNo = x.NxtStockNo ?? string.Empty,
                NxtOrderNo = x.NxtOrderNo ?? string.Empty,
                NxtOrderRefNo = x.NxtOrderRefNo ?? string.Empty,
                NxtInvoiceNo = x.NxtInvoiceNo ?? string.Empty,
                AutoInvoice = x.AutoInvoice ?? false,
                EnterPinOnTill = x.EnterPinOnTill ?? false,
                CreaditOnDayAcc = x.CreaditOnDayAcc ?? false,
                NxtBatchNo = x.NxtBatchNo ?? string.Empty,
                NxtQuoteNo = x.NxtQuoteNo ?? string.Empty,
                TillMsg = x.TillMsg ?? string.Empty,
                InvoiceMsg = x.InvoiceMsg ?? string.Empty,
                ServiceMessage = x.ServiceMessage ?? string.Empty,
                AutoStockNo = x.AutoStockNo ?? false,
                MakeOnInvice = x.MakeOnInvice ?? false,
                DefaultWebProd = x.DefaultWebProd ?? false,
                WhareHouseShopNo = x.WhareHouseShopNo ?? string.Empty,
                BaseCurrency = x.BaseCurrency ?? string.Empty,
                AllowSeparateComboItemReturn = x.AllowSeparateComboItemReturn ?? true,
                ComboPartialReturnRefundMode = x.ComboPartialReturnRefundMode ?? "ALLOCATED",
                DiscountPartNumber = x.DiscountPartNumber ?? string.Empty,
                ReturnStockMode = string.IsNullOrWhiteSpace(x.ReturnStockMode)
                    ? ReturnStockModes.AskUserEveryReturn
                    : x.ReturnStockMode
            };
        }

        private static void Apply(SysOptionsDto dto, SysOption entity)
        {
            entity.Password1 = dto.Password1?.Trim() ?? string.Empty;
            entity.Password2 = dto.Password2?.Trim() ?? string.Empty;
            entity.Password3 = dto.Password3?.Trim() ?? string.Empty;
            entity.PurchaseOrderMessage = dto.PurchaseOrderMessage?.Trim() ?? string.Empty;
            entity.OrderRecievedIgnoreFrameEntry = dto.OrderRecievedIgnoreFrameEntry;
            entity.SalesAnalysisXlsx = dto.SalesAnalysisXlsx;
            entity.StockTurnValue = dto.StockTurnValue;
            entity.RoundDownWebRRPVal = dto.RoundDownWebRRPVal;
            entity.RoundUpBoxQty = dto.RoundUpBoxQty;
            entity.UpdateSupplierFromStockInput = dto.UpdateSupplierFromStockInput;
            entity.UpdateWebsiteOrderReceived = dto.UpdateWebsiteOrderReceived;
            entity.UseTemplateMessages = dto.UseTemplateMessages;
            entity.POPrintSearch1 = dto.POPrintSearch1;
            entity.POPrintSearch2 = dto.POPrintSearch2;
            entity.POPrintDescription = dto.POPrintDescription;
            entity.POPrintSize = dto.POPrintSize;
            entity.POPrintColor = dto.POPrintColor;
            entity.POEmailOrderToSupplier = dto.POEmailOrderToSupplier;
            entity.Layaway = dto.Layaway;
            entity.KeywordProductSearch = dto.KeywordProductSearch;
            entity.LayawayReservedForDays = dto.LayawayReservedForDays;
            entity.LabelByUser = dto.LabelByUser;
            entity.GenerateInvoiceOnCollection = dto.GenerateInvoiceOnCollection;
            entity.GenerateInvoiceOnComplete = dto.GenerateInvoiceOnComplete;
            entity.ZPrintCashBreakdown = dto.ZPrintCashBreakdown;
            entity.ZPrintFloatBreakdown = dto.ZPrintFloatBreakdown;
            entity.ZPrintOtherTotals = dto.ZPrintOtherTotals;
            entity.ZPrintPaymentAnalysis = dto.ZPrintPaymentAnalysis;
            entity.ZPrintSalesAnalysis = dto.ZPrintSalesAnalysis;
            entity.ZPrintVatAnalysis = dto.ZPrintVatAnalysis;
            entity.ZShowTotalsOnEntry = dto.ZShowTotalsOnEntry;
            entity.BuildBikeComplete = dto.BuildBikeComplete;
            entity.CurrentYear = dto.CurrentYear?.Trim() ?? string.Empty;
            entity.QuickStockInput = dto.QuickStockInput;
            entity.UpdateCostOnInput = dto.UpdateCostOnInput;
            entity.CreateModeIfMFRNotFound = dto.CreateModeIfMFRNotFound;
            entity.UseLabelManager = dto.UseLabelManager;
            entity.CustomerOrderPrint = dto.CustomerOrderPrint;
            entity.RaiseCustomerOrdersOnSave = dto.RaiseCustomerOrdersOnSave;
            entity.TillPaymentCardId = dto.TillPaymentCardId?.Trim() ?? string.Empty;
            entity.TillPaymentICardId = dto.TillPaymentICardId?.Trim() ?? string.Empty;
            entity.POPrintNotes = dto.POPrintNotes;
            entity.CustomerOrderConfirmationOpenInMail = dto.CustomerOrderConfirmationOpenInMail;
            entity.LabelPrintDescription = dto.LabelPrintDescription;
            entity.BACsDetails = dto.BACsDetails?.Trim() ?? string.Empty;
            entity.POAutoReorderByMin = dto.POAutoReorderByMin;
            entity.POAutoReorderByMax = dto.POAutoReorderByMax;
            entity.WebPrice99 = dto.WebPrice99;
            entity.POAutoReorderLocWiseSplitEnabled = dto.POAutoReorderLocWiseSplitEnabled;
            entity.PrintPDIOnSale = dto.PrintPDIOnSale;
            entity.WorkshopCustomPartsEnabled = dto.WorkshopCustomPartsEnabled;
            entity.WorkshopPart1 = dto.WorkshopPart1?.Trim() ?? string.Empty;
            entity.WorkshopPart1Name = dto.WorkshopPart1Name?.Trim() ?? string.Empty;
            entity.WorkshopPart2 = dto.WorkshopPart2?.Trim() ?? string.Empty;
            entity.WorkshopPart2Name = dto.WorkshopPart2Name?.Trim() ?? string.Empty;
            entity.WorkshopPart3 = dto.WorkshopPart3?.Trim() ?? string.Empty;
            entity.WorkshopPart3Name = dto.WorkshopPart3Name?.Trim() ?? string.Empty;
            entity.WorkshopPart4 = dto.WorkshopPart4?.Trim() ?? string.Empty;
            entity.WorkshopPart4Name = dto.WorkshopPart4Name?.Trim() ?? string.Empty;
            entity.WorkshopPart5 = dto.WorkshopPart5?.Trim() ?? string.Empty;
            entity.WorkshopPart5Name = dto.WorkshopPart5Name?.Trim() ?? string.Empty;
            entity.VeloLifeInsurance = dto.VeloLifeInsurance;
            entity.BikmoInsurance = dto.BikmoInsurance;
            entity.WorkshopCustomerServiceJobCardNarrative = dto.WorkshopCustomerServiceJobCardNarrative?.Trim() ?? string.Empty;
            entity.LabelPrintPromoPrice = dto.LabelPrintPromoPrice;
            entity.CompanyNo = dto.CompanyNo?.Trim() ?? string.Empty;
            entity.CompanyName = dto.CompanyName?.Trim() ?? string.Empty;
            entity.CompanyLogoUrl = dto.CompanyLogoUrl?.Trim() ?? string.Empty;
            entity.QuoteMessage = dto.QuoteMessage?.Trim() ?? string.Empty;
            entity.AutoPrintServiceInv = dto.AutoPrintServiceInv;
            entity.NxtPartNo = dto.NxtPartNo?.Trim() ?? string.Empty;
            entity.NxtDespNo = dto.NxtDespNo?.Trim() ?? string.Empty;
            entity.NxtStockNo = dto.NxtStockNo?.Trim() ?? string.Empty;
            entity.NxtOrderNo = dto.NxtOrderNo?.Trim() ?? string.Empty;
            entity.NxtOrderRefNo = dto.NxtOrderRefNo?.Trim() ?? string.Empty;
            entity.NxtInvoiceNo = dto.NxtInvoiceNo?.Trim() ?? string.Empty;
            entity.AutoInvoice = dto.AutoInvoice;
            entity.EnterPinOnTill = dto.EnterPinOnTill;
            entity.CreaditOnDayAcc = dto.CreaditOnDayAcc;
            entity.NxtBatchNo = dto.NxtBatchNo?.Trim() ?? string.Empty;
            entity.NxtQuoteNo = dto.NxtQuoteNo?.Trim() ?? string.Empty;
            entity.TillMsg = dto.TillMsg?.Trim() ?? string.Empty;
            entity.InvoiceMsg = dto.InvoiceMsg?.Trim() ?? string.Empty;
            entity.ServiceMessage = dto.ServiceMessage?.Trim() ?? string.Empty;
            entity.AutoStockNo = dto.AutoStockNo;
            entity.MakeOnInvice = dto.MakeOnInvice;
            entity.DefaultWebProd = dto.DefaultWebProd;
            entity.WhareHouseShopNo = dto.WhareHouseShopNo?.Trim() ?? string.Empty;
            entity.BaseCurrency = dto.BaseCurrency?.Trim() ?? string.Empty;
            entity.AllowSeparateComboItemReturn = dto.AllowSeparateComboItemReturn;
            entity.ComboPartialReturnRefundMode = string.IsNullOrWhiteSpace(dto.ComboPartialReturnRefundMode)
                ? "ALLOCATED"
                : dto.ComboPartialReturnRefundMode.Trim().ToUpperInvariant();
            entity.DiscountPartNumber = dto.DiscountPartNumber?.Trim() ?? string.Empty;
            entity.ReturnStockMode = string.IsNullOrWhiteSpace(dto.ReturnStockMode)
                ? ReturnStockModes.AskUserEveryReturn
                : dto.ReturnStockMode.Trim().ToUpperInvariant();
        }
    }
}
