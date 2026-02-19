using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Feedback;
using SafeBit.Api.Model;
using System.Security.Claims;

namespace SafeBit.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly SafeBiteDbContext _db;

        public FeedbackController(SafeBiteDbContext db)
        {
            _db = db;
        }

        // Submit feedback report for incorrect dish detection
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(
            [FromBody] SubmitFeedbackRequest request)
        {
            if (request.DishID <= 0)
                return BadRequest("Invalid Dish ID.");

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Feedback message is required.");

            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            // Ensure dish exists and belongs to a menu scanned by this user
            var dish = await _db.Dishes
                .Include(d => d.MenuUpload)
                .FirstOrDefaultAsync(d =>
                    d.DishID == request.DishID &&
                    !d.IsDeleted);

            if (dish == null)
                return NotFound("Dish not found.");

            var hasScanned = await _db.ScanHistories
                .AnyAsync(s =>
                    s.UserID == userId &&
                    s.MenuID == dish.MenuID &&
                    !s.IsDeleted);

            if (!hasScanned)
                return Forbid("You can only report dishes from your scans.");

            // prevent duplicate reports per user per dish
            var alreadyReported = await _db.FeedbackReports
                .AnyAsync(r =>
                    r.UserID == userId &&
                    r.DishID == request.DishID &&
                    !r.IsDeleted);

            if (alreadyReported)
                return BadRequest("You already submitted feedback for this dish.");

            var report = new FeedbackReport
            {
                UserID = userId,
                DishID = request.DishID,
                Message = request.Message.Trim(),
                Status = Model.Enums.FeedbackStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                UploadDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.FeedbackReports.Add(report);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Feedback submitted successfully.",
                reportId = report.ReportID,
                status = report.Status.ToString()
            });
        }
    }
}
