namespace SafeBit.Api.Model
{
    public class Dish
    {
        public int DishID { get; set; }

        public int MenuID { get; set; }
        public MenuUpload MenuUpload { get; set; } = null!;

        public string DishName { get; set; } = null!;
        public bool IsSafe { get; set; } // from ERD: IsSafe
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
        public ICollection<FeedbackReport> FeedbackReports { get; set; } = new List<FeedbackReport>();
    }
}
