using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Menu;
using SafeBit.Api.Services;
using System.Security.Claims;

namespace SafeBit.Api.Controllers
{
    [Authorize]
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
                return BadRequest("Menu image is required.");


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
                Diseases = diseases
            };

      
            var aiResult = await _ai.AnalyzeMenuAsync(
                request.File,
                profile
            );


            var menuId = await _analysisService.CreateMenuAndSaveResultAsync(
                userId,
                request.RestaurantName.Trim(),
                request.File,
                aiResult
            );


            return Ok(new
            {
                menuId,
                aiResult
            });
        }
    }
}
