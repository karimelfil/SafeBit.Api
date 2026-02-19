using SafeBit.Api.Model.Enums;

namespace SafeBit.Api.Model
{
    public class FeedbackReport
    {
        public int ReportID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; } = null!;

        public int DishID { get; set; }
        public Dish Dish { get; set; } = null!;

        public string Message { get; set; } = null!;

        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
