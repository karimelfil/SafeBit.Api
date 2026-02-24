namespace SafeBit.Api.DTOs.HealthAnalytics
{
    public class KeyInsightDto
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;  // "info" | "warning" | "success" | "primary" (for UI colors)
    }
}
