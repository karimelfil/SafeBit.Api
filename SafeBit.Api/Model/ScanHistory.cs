namespace SafeBit.Api.Model
{
    public class ScanHistory
    {

        public int ScanID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; } = null!;

        public int MenuID { get; set; }
        public MenuUpload MenuUpload { get; set; } = null!;

        public DateTime ScanDate { get; set; } = DateTime.UtcNow;
        public string? ResultsSummary { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    }
}
