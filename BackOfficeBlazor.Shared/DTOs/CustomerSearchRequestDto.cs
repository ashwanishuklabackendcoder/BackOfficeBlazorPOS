namespace BackOfficeBlazor.Shared.DTOs
{
    public class CustomerSearchRequestDto
    {
        public string? Surname { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Initials { get; set; }
        public string? Address { get; set; }
        public string? AccNo { get; set; }
        public string? PostCode { get; set; }
        public string? LoyaltyNo { get; set; }
        public bool CurrentOnly { get; set; }
        public string? Mobile { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string MatchType { get; set; } = "Contains";
    }
}
