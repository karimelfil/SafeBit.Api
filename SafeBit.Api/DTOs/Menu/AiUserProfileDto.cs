using System.Text.Json.Serialization;

namespace SafeBit.Api.DTOs.Menu
{
    public class AiUserProfileDto
    {
        [JsonPropertyName("allergies")]
        public List<string> Allergies { get; set; } = new();

        [JsonPropertyName("diseases")]
        public List<string> Diseases { get; set; } = new();

        [JsonPropertyName("is_pregnant")]
        public bool IsPregnant { get; set; }
    }
}
