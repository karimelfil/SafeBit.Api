namespace SafeBit.Api.DTOs.Feedback
{
    public class FeedbackReportListItemDto
    {
        public string ReportID { get; set; } = null!;
        public string DishName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
    }
}
