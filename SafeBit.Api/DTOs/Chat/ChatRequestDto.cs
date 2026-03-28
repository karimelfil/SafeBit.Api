using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SafeBit.Api.DTOs.Chat
{
    public class ChatRequestDto
    {
        [Required]
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }
    }

    public class AiChatRequestDto
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("user_profile")]
        public AiChatUserProfileDto UserProfile { get; set; } = new();

        [JsonPropertyName("request_id")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("scan_history")]
        public List<AiChatScanHistoryItemDto> ScanHistory { get; set; } = [];

        [JsonPropertyName("use_session_memory")]
        public bool UseSessionMemory { get; set; } = true;

        [JsonPropertyName("include_memory")]
        public bool IncludeMemory { get; set; } = false;
    }

    public class AiChatUserProfileDto
    {
        [JsonPropertyName("allergies")]
        public List<string> Allergies { get; set; } = [];

        [JsonPropertyName("intolerances")]
        public List<string> Intolerances { get; set; } = [];

        [JsonPropertyName("diseases")]
        public List<string> Diseases { get; set; } = [];

        [JsonPropertyName("forbidden_ingredients")]
        public List<string> ForbiddenIngredients { get; set; } = [];

        [JsonPropertyName("dietary_preferences")]
        public List<string> DietaryPreferences { get; set; } = [];

        [JsonPropertyName("is_pregnant")]
        public bool IsPregnant { get; set; }
    }

    public class AiChatScanHistoryItemDto
    {
        [JsonPropertyName("menu_upload_id")]
        public string MenuUploadId { get; set; } = string.Empty;

        [JsonPropertyName("restaurant_name")]
        public string? RestaurantName { get; set; }

        [JsonPropertyName("scanned_at")]
        public DateTime ScannedAt { get; set; }

        [JsonPropertyName("extracted_text_preview")]
        public string? ExtractedTextPreview { get; set; }

        [JsonPropertyName("dishes")]
        public List<AiChatHistoryDishDto> Dishes { get; set; } = [];
    }

    public class AiChatHistoryDishDto
    {
        [JsonPropertyName("dish_name")]
        public string DishName { get; set; } = string.Empty;

        [JsonPropertyName("detected_triggers")]
        public List<string> DetectedTriggers { get; set; } = [];

        [JsonPropertyName("ingredients_found")]
        public List<string> IngredientsFound { get; set; } = [];

        [JsonPropertyName("safety_level")]
        public string SafetyLevel { get; set; } = "CAUTION";

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("ingredient_coverage")]
        public double IngredientCoverage { get; set; }

        [JsonPropertyName("needs_user_confirmation")]
        public bool NeedsUserConfirmation { get; set; }

        [JsonPropertyName("conflicts")]
        public List<AiChatConflictDto> Conflicts { get; set; } = [];

        [JsonPropertyName("notes")]
        public List<string> Notes { get; set; } = [];
    }

    public class AiChatConflictDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("trigger")]
        public string Trigger { get; set; } = string.Empty;

        [JsonPropertyName("evidence")]
        public string Evidence { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
    }

    public class AiChatPythonResponseDto
    {
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("dish_name")]
        public string? DishName { get; set; }

        [JsonPropertyName("reasoning_summary")]
        public List<string> ReasoningSummary { get; set; } = [];

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = [];

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    public class ChatResponseDto
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;
    }
}
