using System.Text.Json.Serialization;

namespace SafeBit.Api.DTOs.Menu
{
    public class AiDishDto
    {
        [JsonPropertyName("dish_name")]
        public string DishName { get; set; } = null!;

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
        public List<AiConflictDto> Conflicts { get; set; } = [];

        [JsonPropertyName("notes")]
        public List<string> Notes { get; set; } = [];
    }

    public class AiConflictDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("trigger")]
        public string Trigger { get; set; } = null!;

        [JsonPropertyName("evidence")]
        public string Evidence { get; set; } = null!;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = null!;
    }
}
