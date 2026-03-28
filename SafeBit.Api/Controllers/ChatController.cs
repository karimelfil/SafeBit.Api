using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Chat;
using SafeBit.Api.DTOs.Menu;
using SafeBit.Api.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SafeBit.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly SafeBiteDbContext _db;
        private readonly AiAgentService _ai;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ChatController(SafeBiteDbContext db, AiAgentService ai)
        {
            _db = db;
            _ai = ai;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message is required.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var userProfile = await BuildUserProfileAsync(userId);
            if (userProfile == null)
                return NotFound("User not found.");

            var trimmedQuestion = request.Message.Trim();
            var useSessionMemory = ShouldUseSessionMemory(trimmedQuestion);
            var aiRequest = new AiChatRequestDto
            {
                Question = trimmedQuestion,
                RequestId = Guid.NewGuid().ToString("N"),
                UserId = userId.ToString(),
                SessionId = string.IsNullOrWhiteSpace(request.SessionId)
                    ? Guid.NewGuid().ToString("N")
                    : request.SessionId.Trim(),
                UserProfile = userProfile,
                ScanHistory = await BuildScanHistoryAsync(userId),
                UseSessionMemory = useSessionMemory,
                IncludeMemory = false
            };

            var response = await _ai.ChatAsync(aiRequest);

            return Ok(new ChatResponseDto
            {
                Question = trimmedQuestion,
                Answer = BuildAnswer(response)
            });
        }

        private async Task<AiChatUserProfileDto?> BuildUserProfileAsync(int userId)
        {
            var user = await _db.Users
                .AsNoTracking()
                .Where(u => u.UserID == userId && !u.IsDeleted)
                .Select(u => new
                {
                    u.IsPregnant
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            var allergies = await (
                from ua in _db.UserAllergies
                join a in _db.Allergies on ua.AllergyID equals a.AllergyID
                where ua.UserID == userId && !ua.IsDeleted && !a.IsDeleted
                select a.Name
            )
            .AsNoTracking()
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

            var diseases = await (
                from ud in _db.UserDiseases
                join d in _db.Diseases on ud.DiseaseID equals d.DiseaseID
                where ud.UserID == userId && !ud.IsDeleted && !d.IsDeleted
                select d.Name
            )
            .AsNoTracking()
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

            return new AiChatUserProfileDto
            {
                Allergies = allergies,
                Intolerances = [],
                Diseases = diseases,
                ForbiddenIngredients = [],
                DietaryPreferences = [],
                IsPregnant = user.IsPregnant
            };
        }

        private async Task<List<AiChatScanHistoryItemDto>> BuildScanHistoryAsync(int userId)
        {
            var scans = await _db.ScanHistories
                .AsNoTracking()
                .Where(s => s.UserID == userId && !s.IsDeleted)
                .OrderByDescending(s => s.ScanDate)
                .Take(10)
                .Select(s => new
                {
                    s.MenuID,
                    s.ScanDate,
                    s.ResultsSummary,
                    RestaurantName = s.MenuUpload.RestaurantName
                })
                .ToListAsync();

            return scans.Select(scan =>
            {
                var aiResult = TryDeserializeAiResult(scan.ResultsSummary);

                return new AiChatScanHistoryItemDto
                {
                    MenuUploadId = scan.MenuID.ToString(),
                    RestaurantName = CleanRestaurantName(scan.RestaurantName),
                    ScannedAt = scan.ScanDate,
                    ExtractedTextPreview = BuildExtractedPreview(aiResult, scan.ResultsSummary),
                    Dishes = BuildHistoryDishes(aiResult)
                };
            }).ToList();
        }

        private List<AiChatHistoryDishDto> BuildHistoryDishes(AiAnalyzeMenuResponse? aiResult)
        {
            if (aiResult?.Dishes == null || aiResult.Dishes.Count == 0)
                return [];

            return aiResult.Dishes
                .Where(d => !string.IsNullOrWhiteSpace(d.DishName))
                .Select(d => new AiChatHistoryDishDto
                {
                    DishName = CleanDishName(d.DishName),
                    DetectedTriggers = CleanList(d.DetectedTriggers),
                    IngredientsFound = CleanList(d.IngredientsFound),
                    SafetyLevel = string.IsNullOrWhiteSpace(d.SafetyLevel) ? "CAUTION" : d.SafetyLevel,
                    Confidence = d.Confidence,
                    IngredientCoverage = d.IngredientCoverage,
                    NeedsUserConfirmation = d.NeedsUserConfirmation,
                    Conflicts = d.Conflicts
                        .Select(c => new AiChatConflictDto
                        {
                            Type = c.Type,
                            Trigger = c.Trigger,
                            Evidence = c.Evidence,
                            Explanation = c.Explanation
                        })
                        .ToList(),
                    Notes = CleanList(d.Notes)
                })
                .Where(d => !string.IsNullOrWhiteSpace(d.DishName))
                .GroupBy(d => d.DishName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }

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

        private static string? BuildExtractedPreview(AiAnalyzeMenuResponse? aiResult, string? resultsSummary)
        {
            var summary = aiResult?.Summary?.ShortSummary;
            if (!string.IsNullOrWhiteSpace(summary))
                return summary;

            var preview = aiResult?.ExtractedTextPreview;
            if (!string.IsNullOrWhiteSpace(preview))
            {
                preview = preview.ReplaceLineEndings(" ").Trim();
                return preview.Length <= 240 ? preview : preview[..240];
            }

            if (string.IsNullOrWhiteSpace(resultsSummary))
                return null;

            var compact = resultsSummary.ReplaceLineEndings(" ").Trim();
            return compact.Length <= 240
                ? compact
                : compact[..240];
        }

        private static string BuildAnswer(AiChatPythonResponseDto response)
        {
            if (!string.IsNullOrWhiteSpace(response.Explanation))
                return response.Explanation!;

            if (response.ReasoningSummary.Count > 0)
                return string.Join(" ", response.ReasoningSummary);

            if (!string.IsNullOrWhiteSpace(response.Status))
                return $"Status: {response.Status}.";

            return "No answer was returned by the AI service.";
        }

        private static bool ShouldUseSessionMemory(string question)
        {
            var normalized = question.Trim().ToLowerInvariant();

            string[] followUpMarkers =
            [
                "why is that",
                "why that",
                "that dish",
                "that one",
                "the one you mentioned",
                "the one you recommended",
                "that restaurant",
                "those dishes",
                "what about that",
                "why",
                "explain more",
                "tell me more"
            ];

            return followUpMarkers.Any(marker => normalized.Contains(marker));
        }

        private static List<string> CleanList(IEnumerable<string>? values)
        {
            if (values == null)
                return [];

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string? CleanRestaurantName(string? restaurantName)
        {
            if (string.IsNullOrWhiteSpace(restaurantName))
                return restaurantName;

            var cleaned = restaurantName.Trim();
            cleaned = cleaned.Replace('_', ' ');
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "\\s+", " ");

            if (!IsLikelyValidRestaurantName(cleaned))
                return null;

            return cleaned;
        }

        private static string CleanDishName(string? dishName)
        {
            if (string.IsNullOrWhiteSpace(dishName))
                return string.Empty;

            var cleaned = dishName.Trim();

            if (cleaned.StartsWith("Dish:", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[5..].Trim();

            var duplicateMarkerIndex = cleaned.IndexOf(" Dish Name:", StringComparison.OrdinalIgnoreCase);
            if (duplicateMarkerIndex > 0)
                cleaned = cleaned[..duplicateMarkerIndex].Trim();

            cleaned = cleaned.Replace('_', ' ');
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "\\s+", " ");

            return cleaned;
        }

        private static bool IsLikelyValidRestaurantName(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length < 3)
                return false;

            var lower = normalized.ToLowerInvariant();

            string[] blockedFragments =
            [
                "test",
                "pipoppo",
                "jhgy",
                "asdf",
                "qwer",
                "zxcv"
            ];

            if (blockedFragments.Any(lower.Contains))
                return false;

            var letters = normalized.Where(char.IsLetter).Select(char.ToLowerInvariant).ToList();
            if (letters.Count == 0)
                return false;

            var distinctRatio = letters.Distinct().Count() / (double)letters.Count;
            if (letters.Count >= 12 && distinctRatio < 0.35)
                return false;

            return true;
        }
    }
}
