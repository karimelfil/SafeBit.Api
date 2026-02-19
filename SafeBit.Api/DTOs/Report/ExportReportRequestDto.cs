namespace SafeBit.Api.DTOs.Report
{
    public class ExportReportRequestDto
    {
        public string ReportType { get; set; } = null!;
        public string DateRange { get; set; } = "Last30Days";
        public string Format { get; set; } = "Excel";

    }
}
