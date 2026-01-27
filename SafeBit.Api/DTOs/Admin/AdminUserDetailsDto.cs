namespace SafeBit.Api.DTOs.Admin
{
    public class AdminUserDetailsDto
    {
        public int User_Id { get; set; }

        public string Email { get; set; } = string.Empty;
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public string? Phone { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime Date_Of_Birth { get; set; }
        public string Gender { get; set; } = string.Empty;

        public DateTime Registration_Date { get; set; }
    }
}
