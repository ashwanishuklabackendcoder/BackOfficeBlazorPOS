namespace BackOfficeBlazor.Shared.DTOs
{
    public enum PrinterMode
    {
        Windows = 0,
        Tcp = 1,
        File = 2,
        Email = 3
    }

    public enum PrinterType
    {
        Receipt = 0,
        Label = 1,
        A4 = 2
    }

    public enum LabelFormat
    {
        None = 0,
        Zpl = 1
    }

    public enum PrintJobStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
