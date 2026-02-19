using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using System.Security.Claims;

namespace SafeBit.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/scan")]
    public class ScanController : ControllerBase
    {
        private readonly SafeBiteDbContext _db;

        public ScanController(SafeBiteDbContext db)
        {
            _db = db;
        }

        // Get user's scan history with restaurant name and dish counts
        [HttpGet("history")]
        public async Task<IActionResult> GetScanHistory()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var history = await _db.ScanHistories
                .Where(s => s.UserID == userId && !s.IsDeleted)
                .OrderByDescending(s => s.ScanDate)
                .Select(s => new
                {
                    s.ScanID,
                    RestaurantName = s.MenuUpload.RestaurantName,
                    s.ScanDate,

                    SafeCount = s.MenuUpload.Dishes
                        .Count(d => d.IsSafe && !d.IsDeleted),

                    UnsafeCount = s.MenuUpload.Dishes
                        .Count(d => !d.IsSafe && !d.IsDeleted),

                    RiskyCount = 0 
                })
                .ToListAsync();

            return Ok(history);
        }


        // Get details of a specific scan, including dishes and ingredients
        [HttpGet("{scanId}")]
        public async Task<IActionResult> GetMenuDetails(int scanId)
        {
            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var scan = await _db.ScanHistories
                .Where(s => s.ScanID == scanId &&
                            s.UserID == userId &&
                            !s.IsDeleted)
                .Select(s => new
                {
                    s.ScanID,
                    s.ScanDate,
                    RestaurantName = s.MenuUpload.RestaurantName,
                    FilePath = s.MenuUpload.FilePath,

                    Dishes = s.MenuUpload.Dishes
                        .Where(d => !d.IsDeleted)
                        .Select(d => new
                        {
                            d.DishID,
                            d.DishName,
                            SafetyStatus = d.IsSafe ? "SAFE" : "UNSAFE",

                            Ingredients = d.DishIngredients
                                .Where(di => !di.IsDeleted)
                                .Select(di => di.Ingredient.Name)
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (scan == null)
                return NotFound("Scan not found.");

            return Ok(scan);
        }

    }
}
