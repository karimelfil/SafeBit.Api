namespace SafeBit.Api.DTOs.HealthAnalytics
{
    public class MonthlyTrendDto
    {
        public string Month { get; set; } = null!; 
        public int Allergies { get; set; }
        public int Diseases { get; set; }
    }
}
