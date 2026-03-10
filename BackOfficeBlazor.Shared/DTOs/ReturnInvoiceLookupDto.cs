namespace BackOfficeBlazor.Shared.DTOs;

public class ReturnInvoiceLookupDto
{
    public string InvoiceNo { get; set; } = "";
    public DateTime? InvoiceDate { get; set; }
    public string CustomerAccNo { get; set; } = "";
}
