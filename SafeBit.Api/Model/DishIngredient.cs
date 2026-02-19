namespace SafeBit.Api.Model
{
    public class DishIngredient
    {
        public int DishID { get; set; }
        public Dish Dish { get; set; } = null!;

        public int IngredientID { get; set; }
        public Ingredient Ingredient { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
