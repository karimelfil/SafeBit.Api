namespace SafeBit.Api.DTOs.Feedback
{
    public class FeedbackReportDetailsDto
    {
        public string ReportID { get; set; } = null!;

        public string Status { get; set; } = null!;  

        public string UserEmail { get; set; } = null!;
        public string UserID { get; set; } = null!;   

        public string DishName { get; set; } = null!;
        public string DishID { get; set; } = null!;   

        public DateTime SubmittedAt { get; set; }

        public string ReportMessage { get; set; } = null!;
    }
}
