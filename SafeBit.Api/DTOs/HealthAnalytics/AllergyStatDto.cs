namespace SafeBit.Api.DTOs.HealthAnalytics
{
    public class AllergyStatDto
    {
        public int AllergyID { get; set; }
        public string Name { get; set; } = null!;
        public int AffectedUsers { get; set; }
        public double PercentOfTotalUsers { get; set; }
    }
}
