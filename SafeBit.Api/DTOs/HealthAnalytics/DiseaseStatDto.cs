namespace SafeBit.Api.DTOs.HealthAnalytics
{
    public class DiseaseStatDto
    {
        public int DiseaseID { get; set; }         // for "Other" we set DiseaseID = 0
        public string Name { get; set; } = null!;
        public int AffectedUsers { get; set; }
        public double PercentOfTotalUsers { get; set; }
    }
}
