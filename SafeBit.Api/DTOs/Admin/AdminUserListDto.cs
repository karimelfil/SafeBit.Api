namespace SafeBit.Api.DTOs.Admin
{
    public class AdminUserListDto
    {
        public int User_Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Registration_Date { get; set; }
    }
}
