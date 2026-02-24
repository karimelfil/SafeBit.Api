namespace SafeBit.Api.DTOs.Dish
{
    public class DishIngredientsResponseDto
    {
        public string DishID { get; set; } = null!;
        public string UploadedBy { get; set; } = null!;
        public DateTime UploadDate { get; set; }
        public List<IngredientDto> DetectedIngredients { get; set; } = new();
    }
}
