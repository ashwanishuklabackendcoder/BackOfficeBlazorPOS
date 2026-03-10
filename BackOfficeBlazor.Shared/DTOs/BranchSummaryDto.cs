namespace BackOfficeBlazor.Shared.DTOs
{
    public class BranchSummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool KeyLocation { get; set; }
    }
}
