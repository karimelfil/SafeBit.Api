namespace SafeBit.Api.DTOs.AiAgent
{
    public class AiAnalyzeResponseDto
    {
        public string MenuUploadId { get; set; } = "";
        public string ExtractedTextPreview { get; set; } = "";
        public List<AiDishResultDto> Dishes { get; set; } = new();
    }

    public class AiDishResultDto
    {
        public string DishName { get; set; } = "";
        public List<string> DetectedTriggers { get; set; } = new();
        public List<string> IngredientsFound { get; set; } = new();
        public string SafetyLevel { get; set; } = "CAUTION"; 
        public double Confidence { get; set; } = 0.0;
        public List<AiConflictDto> Conflicts { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }

    public class AiConflictDto
    {
        public string Type { get; set; } = "";
        public string Trigger { get; set; } = "";
        public string Evidence { get; set; } = "";
        public string Explanation { get; set; } = "";
    }

}
