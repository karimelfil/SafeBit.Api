namespace SafeBit.Api.DTOs.Menu
{
    public class AiDishResult
    {
        public string DishName { get; set; } = null!;
        public List<string> IngredientsFound { get; set; } = new();
        public string SafetyLevel { get; set; } = null!; // SAFE / RISKY / CAUTION
        public double Confidence { get; set; }
    }
}
