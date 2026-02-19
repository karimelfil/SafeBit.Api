namespace SafeBit.Api.DTOs.Feedback
{
    public class SubmitFeedbackRequest
    {
        public int DishID { get; set; }
        public string Message { get; set; } = null!;
    }
}
