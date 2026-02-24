namespace SafeBit.Api.DTOs.Dish
{
    public class DishListItemDto
    {
        public string DishID { get; set; } = null!;   // "DISH001"
        public string DishName { get; set; } = null!;
        public string Restaurant { get; set; } = null!;
        public string UploadedBy { get; set; } = null!;
        public DateTime UploadDate { get; set; }
    }
}
