using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Menu;
using SafeBit.Api.Services;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace SafeBit.Api.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly AiAgentService _ai;
        private readonly SafeBiteDbContext _db;
        private readonly MenuAnalysisService _analysisService;

        public MenuController(
            AiAgentService ai,
            SafeBiteDbContext db,
            MenuAnalysisService analysisService)
        {
            _ai = ai;
            _db = db;
            _analysisService = analysisService;
        }

        // Upload menu image and analyze it with AI
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMenu(
            [FromForm] MenuUploadRequest request)
        {

            if (request.File == null || request.File.Length == 0)
                return BadRequest("Menu is required.");


            if (string.IsNullOrWhiteSpace(request.RestaurantName))
                return BadRequest("Restaurant name is required.");


            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "application/pdf"
            };

            if (!allowedTypes.Contains(request.File.ContentType))
                return BadRequest("Unsupported file type. Use JPG, PNG, or PDF.");


            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var user = await _db.Users
                .AsNoTracking()
                .Where(u => u.UserID == userId && !u.IsDeleted)
                .Select(u => new
                {
                    u.IsPregnant
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            var allergies = await (
                from ua in _db.UserAllergies
                join a in _db.Allergies on ua.AllergyID equals a.AllergyID
                where ua.UserID == userId &&
                      !ua.IsDeleted &&
                      !a.IsDeleted
                select a.Name
            ).ToListAsync();

            var diseases = await (
                from ud in _db.UserDiseases
                join d in _db.Diseases on ud.DiseaseID equals d.DiseaseID
                where ud.UserID == userId &&
                      !ud.IsDeleted &&
                      !d.IsDeleted
                select d.Name
            ).ToListAsync();


            var profile = new AiUserProfileDto
            {
                Allergies = allergies,
                Diseases = diseases,
                IsPregnant = user.IsPregnant
            };

      
            var aiResult = await _ai.AnalyzeMenuAsync(
                request.File,
                profile
            );

            NormalizeAiResult(aiResult, profile);


            var menuId = await _analysisService.CreateMenuAndSaveResultAsync(
                userId,
                request.RestaurantName.Trim(),
                request.File,
                aiResult
            );

                    var savedDishes = await _db.Dishes
            .Where(d => d.MenuID == menuId && !d.IsDeleted)
            .Select(d => new
            {
                dishID = d.DishID,
                dishName = d.DishName
            })
            .ToListAsync();

            return Ok(new
            {
                menuId,
                aiResult,
                summary = aiResult.Summary,
                dishes = savedDishes
            });



        }


        // Get analysis result for a specific menu
        private static void NormalizeAiResult(AiAnalyzeMenuResponse aiResult, AiUserProfileDto profile)
        {
            foreach (var dish in aiResult.Dishes)
            {
                NormalizeDish(dish, profile);
            }

            aiResult.Summary = BuildSummary(aiResult.Dishes);
        }


        // Adjust dish safety based on user profile and ingredients
        private static void NormalizeDish(AiDishDto dish, AiUserProfileDto profile)
        {
            var ingredients = dish.IngredientsFound
                .Concat(dish.PredictedIngredients)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            if (IsSafe(dish.SafetyLevel) &&
                UserNeedsSaltCaution(profile) &&
                ingredients.Any(ContainsSaltMarker))
            {
                dish.SafetyLevel = "risky";


                var cautionNote = "Caution: this dish contains salt, so it should be limited rather than treated as fully safe.";
                if (!dish.Notes.Contains(cautionNote, StringComparer.OrdinalIgnoreCase))
                {
                    dish.Notes.Add(cautionNote);
                }

                dish.ShortSummary ??= $"{dish.DishName} contains salt, so this user should limit it rather than treat it as fully safe.";
            }
        }


        // Build a summary of the menu analysis results
        private static AiMenuSummaryDto BuildSummary(IEnumerable<AiDishDto> dishes)
        {
            var safeToOrder = dishes
                .Where(d => IsSafe(d.SafetyLevel))
                .Select(d => d.DishName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var cautionDishes = dishes
                .Where(d => IsCaution(d.SafetyLevel))
                .Select(d => d.DishName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var riskyDishes = dishes
                .Where(d => IsRisky(d.SafetyLevel))
                .Select(d => d.DishName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var parts = new List<string>();
            if (safeToOrder.Count > 0)
                parts.Add($"Safer choices for you: {string.Join(", ", safeToOrder)}.");
            if (riskyDishes.Count > 0)
                parts.Add($"Best to avoid: {string.Join(", ", riskyDishes)}.");
            if (cautionDishes.Count > 0)
                parts.Add($"Check ingredients or ask before ordering: {string.Join(", ", cautionDishes)}.");

            return new AiMenuSummaryDto
            {
                SafeToOrder = safeToOrder,
                CautionDishes = cautionDishes,
                RiskyDishes = riskyDishes,
                ShortSummary = parts.Count == 0 ? null : string.Join(" ", parts)
            };
        }


        // Determine if user profile indicates a need for salt caution
        private static bool UserNeedsSaltCaution(AiUserProfileDto profile)
        {
            IEnumerable<string> indicators = profile.Allergies.Concat(profile.Diseases);

            return indicators.Any(value =>
            {
                var normalized = value.Trim().ToLowerInvariant();
                return normalized.Contains("salt") ||
                       normalized.Contains("sodium") ||
                       normalized.Contains("hypertension") ||
                       normalized.Contains("high blood pressure");
            });
        }


        // Check if an ingredient contains salt markers
        private static bool ContainsSaltMarker(string ingredient)
        {
            var normalized = ingredient.Trim().ToLowerInvariant();
            return normalized.Contains("salt") || normalized.Contains("sodium");
        }



        // Helper methods to check safety levels

        private static bool IsSafe(string? level) =>
            string.Equals(level, "safe", StringComparison.OrdinalIgnoreCase);

        private static bool IsCaution(string? level) =>
            string.Equals(level, "risky", StringComparison.OrdinalIgnoreCase);

        private static bool IsRisky(string? level) =>
            string.Equals(level, "unsafe", StringComparison.OrdinalIgnoreCase);

    }
}
