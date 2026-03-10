namespace BackOfficeBlazor.Shared.DTOs
{
    public class PrintJobDto
    {
        public int Id { get; set; }
        public string TerminalCode { get; set; } = "";
        public int PrinterConfigId { get; set; }
        public string JobType { get; set; } = "";
        public string Payload { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class PrintJobRequestDto
    {
        public string TerminalCode { get; set; } = "";
        public int PrinterConfigId { get; set; }
        public string JobType { get; set; } = "";
        public string Payload { get; set; } = "";
    }

    public class PrintJobStatusUpdateDto
    {
        public int JobId { get; set; }
        public string Status { get; set; } = "";
        public string? Error { get; set; }
    }
}
