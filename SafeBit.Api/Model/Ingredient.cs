namespace SafeBit.Api.Model
{
    public class Ingredient
    {
        public int IngredientID { get; set; }

        public string Name { get; set; } = null!;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();


    }
}
