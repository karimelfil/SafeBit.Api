namespace SafeBit.Api.DTOs.Report
{
    public class ReportItemDto
    {
        public string Category { get; set; } = null!;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
