using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class OpeningHourDto
    {
        public string Day { get; set; } = string.Empty;
        public TimeOnly? OpenTime { get; set; }
        public TimeOnly? CloseTime { get; set; }
        public bool IsClosed { get; set; }
    }
}
