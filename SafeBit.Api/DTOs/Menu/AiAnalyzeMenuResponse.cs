using System.Text.Json.Serialization;

namespace SafeBit.Api.DTOs.Menu
{
    public class AiAnalyzeMenuResponse
    {
        [JsonPropertyName("menu_upload_id")]
        public string? MenuUploadId { get; set; }

        [JsonPropertyName("extracted_text_preview")]
        public string? ExtractedTextPreview { get; set; }

        [JsonPropertyName("dishes")]
        public List<AiDishDto> Dishes { get; set; } = [];

        [JsonPropertyName("summary")]
        public AiMenuSummaryDto? Summary { get; set; }
    }

    public class AiMenuSummaryDto
    {
        [JsonPropertyName("safe_to_order")]
        public List<string> SafeToOrder { get; set; } = [];

        [JsonPropertyName("caution_dishes")]
        public List<string> CautionDishes { get; set; } = [];

        [JsonPropertyName("risky_dishes")]
        public List<string> RiskyDishes { get; set; } = [];

        [JsonPropertyName("short_summary")]
        public string? ShortSummary { get; set; }
    }
}
