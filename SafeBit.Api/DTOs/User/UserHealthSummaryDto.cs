namespace SafeBit.Api.DTOs.User
{
    public class UserHealthSummaryDto
    {
        public int UserId { get; set; }
        public List<string> Allergies { get; set; } = new();
        public List<string> Diseases { get; set; } = new();
    }
}
