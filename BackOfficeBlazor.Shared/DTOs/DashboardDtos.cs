namespace BackOfficeBlazor.Shared.DTOs
{
    public enum DashboardDatePreset
    {
        Today = 0,
        ThisWeek = 1,
        ThisMonth = 2,
        Custom = 3
    }

    public enum DashboardTrendDirection
    {
        Flat = 0,
        Up = 1,
        Down = 2
    }

    public enum DashboardValueKind
    {
        Currency = 0,
        Percent = 1,
        Count = 2,
        Text = 3
    }

    public class DashboardRequestDto
    {
        public DashboardDatePreset Preset { get; set; } = DashboardDatePreset.ThisMonth;
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<string> Locations { get; set; } = new();
        public string? TerminalCode { get; set; }
    }

    public class DashboardRangeDto
    {
        public DashboardDatePreset Preset { get; set; }
        public string Label { get; set; } = "";
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class DashboardResponseDto
    {
        public DateTime GeneratedAtUtc { get; set; }
        public DashboardRangeDto CurrentRange { get; set; } = new();
        public DashboardRangeDto TodayRange { get; set; } = new();
        public List<DashboardKpiDto> Kpis { get; set; } = new();
        public List<DashboardTrendPointDto> SalesTrend { get; set; } = new();
        public List<DashboardDistributionItemDto> SalesVsReturns { get; set; } = new();
        public List<DashboardDistributionItemDto> PaymentMethods { get; set; } = new();
        public List<DashboardBarItemDto> TopCategories { get; set; } = new();
        public List<DashboardSummaryItemDto> Summary { get; set; } = new();
        public List<DashboardLocationOptionDto> Locations { get; set; } = new();
        public List<DashboardTerminalOptionDto> Terminals { get; set; } = new();
    }

    public class DashboardKpiDto
    {
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string ComparisonLabel { get; set; } = "";
        public DashboardValueKind Kind { get; set; } = DashboardValueKind.Currency;
        public decimal Value { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal Delta { get; set; }
        public decimal DeltaPercent { get; set; }
        public DashboardTrendDirection TrendDirection { get; set; } = DashboardTrendDirection.Flat;
        public bool IsPositiveTrend { get; set; }
    }

    public class DashboardTrendPointDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
    }

    public class DashboardDistributionItemDto
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "";
    }

    public class DashboardBarItemDto
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DashboardSummaryItemDto
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public DashboardValueKind Kind { get; set; } = DashboardValueKind.Text;
        public string? Subtext { get; set; }
    }

    public class DashboardLocationOptionDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Selected { get; set; }
    }

    public class DashboardTerminalOptionDto
    {
        public string Code { get; set; } = "";
        public string? DefaultBranch { get; set; }
        public bool Selected { get; set; }
    }
}
