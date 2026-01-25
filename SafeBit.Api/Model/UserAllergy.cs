namespace SafeBit.Api.Model
{
    public class UserAllergy
    {
        public int UserID { get; set; }
        public int AllergyID { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}
