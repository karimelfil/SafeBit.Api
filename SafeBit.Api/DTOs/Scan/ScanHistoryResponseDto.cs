namespace SafeBit.Api.DTOs.Scan
{
    public class ScanHistoryResponseDto
    {
        public int ScanID { get; set; }
        public string? RestaurantName { get; set; }
        public DateTime ScanDate { get; set; }

        public int SafeCount { get; set; }
        public int UnsafeCount { get; set; }
        public int RiskyCount { get; set; }
    }
}
