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
    }
}
