namespace SafeBit.Api.DTOs.User
{
    public class AddUserDiseasesDto
    {
        public int UserId { get; set; }
        public List<int> DiseaseIds { get; set; } = new();
    }
}
