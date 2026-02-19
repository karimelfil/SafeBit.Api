namespace SafeBit.Api.DTOs.Report
{
    public class GenerateReportRequestDto
    {
        public string ReportType { get; set; } = null!;
        public string DateRange { get; set; } = "Last30Days";
    }
}
