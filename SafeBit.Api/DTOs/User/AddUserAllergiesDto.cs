namespace SafeBit.Api.DTOs.User
{
    public class AddUserAllergiesDto
    {
        public int UserId { get; set; }
        public List<int> AllergyIds { get; set; } = new();
    }
}
