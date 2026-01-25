namespace SafeBit.Api.Model
{
    public class UserDisease
    {
        public int UserID { get; set; }
        public int DiseaseID { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}
