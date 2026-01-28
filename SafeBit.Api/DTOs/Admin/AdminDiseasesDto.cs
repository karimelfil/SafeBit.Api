namespace SafeBit.Api.DTOs.Admin
{
    public class AdminDiseasesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Created_At { get; set; }
    }
}

