using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Dish;

namespace SafeBit.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DishesAdminController : ControllerBase
    {
        private readonly SafeBiteDbContext _context;

        public DishesAdminController(SafeBiteDbContext context)
        {
            _context = context;
        }

        private static string FormatDishCode(int dishId) => $"DISH{dishId:D3}";

        //to be used in admin dashboard for listing all dishes with basic info
        [HttpGet]
        public async Task<ActionResult> GetAllDishes()
        {
            var dishes = await _context.Dishes
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .Include(d => d.MenuUpload)
                .ThenInclude(mu => mu.User)
                .OrderBy(d => d.DishID)
                .Select(d => new DishListItemDto
                {
                    DishID = FormatDishCode(d.DishID),
                    DishName = d.DishName,
                    Restaurant = d.MenuUpload.RestaurantName ?? string.Empty,
                    UploadedBy =
                        ((d.MenuUpload.User.FirstName ?? string.Empty) + " " +
                         (d.MenuUpload.User.LastName ?? string.Empty)).Trim(),
                    UploadDate = d.UploadDate
                })
                .ToListAsync();

            return Ok(dishes);
        }


        //to be used in admin dashboard for showing details of a dish including its ingredients
        [HttpGet("{dishId:int}/ingredients")]
        public async Task<ActionResult> GetDishIngredients(int dishId)
        {
            var dish = await _context.Dishes
                .AsNoTracking()
                .Where(d => !d.IsDeleted && d.DishID == dishId)
                .Include(d => d.MenuUpload)
                .ThenInclude(mu => mu.User)
                .Include(d => d.DishIngredients.Where(di => !di.IsDeleted))
                .ThenInclude(di => di.Ingredient)
                .FirstOrDefaultAsync();

            if (dish == null)
                return NotFound(new { message = "Dish not found." });

            var response = new DishIngredientsResponseDto
            {
                DishID = FormatDishCode(dish.DishID),
                UploadedBy =
                    ((dish.MenuUpload.User.FirstName ?? string.Empty) + " " +
                     (dish.MenuUpload.User.LastName ?? string.Empty)).Trim(),
                UploadDate = dish.UploadDate,
                DetectedIngredients = dish.DishIngredients
                    .Where(di => !di.IsDeleted)
                    .Select(di => new IngredientDto
                    {
                        IngredientID = di.IngredientID,
                        Name = di.Ingredient.Name
                    })
                    .OrderBy(i => i.Name)
                    .ToList()
            };

            return Ok(response);
        }
    }
}
