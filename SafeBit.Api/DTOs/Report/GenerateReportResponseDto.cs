namespace SafeBit.Api.DTOs.Report
{
    public class GenerateReportResponseDto
    {
        public string ReportType { get; set; } = null!;
        public string DateRange { get; set; } = null!;
        public DateTime GeneratedAt { get; set; }

        public int TotalRecords { get; set; }

        public List<ReportItemDto> Data { get; set; } = new();
    }
}
