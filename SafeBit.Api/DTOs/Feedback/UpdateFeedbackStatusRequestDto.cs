using SafeBit.Api.Model.Enums;

namespace SafeBit.Api.DTOs.Feedback
{
    public class UpdateFeedbackStatusRequestDto
    {
        public FeedbackStatus Status { get; set; } 
        public string? UpdatedBy { get; set; }     
    }
}
