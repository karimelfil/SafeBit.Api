namespace SafeBit.Api.DTOs.HealthAnalytics
{
    public class HealthAnalyticsResponseDto
    {
        public int TotalUsers { get; set; }

        public int UsersWithAllergies { get; set; }
        public double UsersWithAllergiesPercent { get; set; }

        public int UsersWithDiseases { get; set; }
        public double UsersWithDiseasesPercent { get; set; }

        public double MonthlyGrowthPercent { get; set; }

        public List<AllergyStatDto> DetailedAllergyStatistics { get; set; } = new();
        public List<DiseaseStatDto> DiseaseDistribution { get; set; } = new();
        public List<MonthlyTrendDto> HealthDataTrends { get; set; } = new();

        public List<KeyInsightDto> KeyInsights { get; set; } = new();
    }
}
