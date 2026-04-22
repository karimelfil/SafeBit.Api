using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Menu;
using System.Security.Claims;
using System.Text.Json;

namespace SafeBit.Api.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/scan")]
    public class ScanController : ControllerBase
    {
        private readonly SafeBiteDbContext _db;

        
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ScanController(SafeBiteDbContext db)
        {
            _db = db;
        }


        
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
                    s.ResultsSummary,
                    Dishes = s.MenuUpload.Dishes
                        .Where(d => !d.IsDeleted)
                        .Select(d => new
                        {
                            d.IsSafe
                        })
                        .ToList()
                })
                .ToListAsync();

            var response = history.Select(s =>
            {
                var aiResult = TryDeserializeAiResult(s.ResultsSummary);
                var safeCount = aiResult?.Summary?.SafeToOrder.Count
                    ?? aiResult?.Dishes.Count(d => IsSafetyLevel(d.SafetyLevel, "safe"))
                    ?? s.Dishes.Count(d => d.IsSafe);
                var riskyCount = aiResult?.Summary?.CautionDishes.Count
                    ?? aiResult?.Dishes.Count(d => IsSafetyLevel(d.SafetyLevel, "risky"))
                    ?? 0;
                var unsafeCount = aiResult?.Summary?.RiskyDishes.Count
                    ?? aiResult?.Dishes.Count(d => IsSafetyLevel(d.SafetyLevel, "unsafe"))
                    ?? s.Dishes.Count(d => !d.IsSafe);

                return new
                {
                    s.ScanID,
                    s.RestaurantName,
                    s.ScanDate,
                    SafeCount = safeCount,
                    RiskyCount = riskyCount,
                    UnsafeCount = unsafeCount
                };
            });

            return Ok(response);
        }


        // Detailed scan results  
        [HttpGet("{scanId}")]
        public async Task<IActionResult> GetMenuDetails(int scanId)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
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
                    s.ResultsSummary,
                    Dishes = s.MenuUpload.Dishes
                        .Where(d => !d.IsDeleted)
                        .Select(d => new
                        {
                            d.DishID,
                            d.DishName,
                            SafetyStatus = d.IsSafe ? "safe" : "risky",
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

            var aiResult = TryDeserializeAiResult(scan.ResultsSummary);
            var aiDishLookup = aiResult?.Dishes
                .GroupBy(d => d.DishName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var dishes = scan.Dishes.Select(d =>
            {
                AiDishDto? aiDish = null;
                aiDishLookup?.TryGetValue(d.DishName.Trim(), out aiDish);

                return new
                {
                    d.DishID,
                    d.DishName,
                    SafetyStatus = aiDish?.SafetyLevel ?? d.SafetyStatus,
                    Ingredients = d.Ingredients,
                    DetectedTriggers = aiDish?.DetectedTriggers ?? [],
                    IngredientsFound = aiDish?.IngredientsFound ?? [],
                    PredictedIngredients = aiDish?.PredictedIngredients ?? [],
                    IngredientPredictionUsed = aiDish?.IngredientPredictionUsed ?? false,
                    Confidence = aiDish?.Confidence,
                    IngredientCoverage = aiDish?.IngredientCoverage,
                    NeedsUserConfirmation = aiDish?.NeedsUserConfirmation ?? false,
                    Conflicts = aiDish?.Conflicts ?? [],
                    Notes = aiDish?.Notes ?? [],
                    ShortSummary = aiDish?.ShortSummary
                };
            });

            return Ok(new
            {
                scan.ScanID,
                scan.ScanDate,
                scan.RestaurantName,
                scan.FilePath,
                Summary = aiResult?.Summary,
                Dishes = dishes
            });
        }


        // Helper method to safely deserialize AI results
        private AiAnalyzeMenuResponse? TryDeserializeAiResult(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<AiAnalyzeMenuResponse>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        // Helper method to determine safety level
        private static bool IsSafetyLevel(string? level, string expectedLevel) =>
            string.Equals(level, expectedLevel, StringComparison.OrdinalIgnoreCase);
    }
}
