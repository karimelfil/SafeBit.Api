namespace SafeBit.Api.Model
{
    public class MenuUpload
    {
        public int MenuID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; } = null!;

        public string? RestaurantName { get; set; }
        public string FilePath { get; set; } = null!;
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
        public ICollection<ScanHistory> ScanHistories { get; set; } = new List<ScanHistory>();
    }
}
