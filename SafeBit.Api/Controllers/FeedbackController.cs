using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Feedback;
using SafeBit.Api.Model;
using SafeBit.Api.Model.Enums;
using SafeBit.Api.Services;
using System.Security.Claims;

namespace SafeBit.Api.Controllers
{

    [ApiController]
    [Route("api/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly SafeBiteDbContext _db;
        private readonly EmailService _emailService;

        public FeedbackController(SafeBiteDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        // Submit feedback report for incorrect dish detection
        [Authorize(Roles = "User")]
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


        private static string FormatReportCode(int reportId) => $"RPT{reportId:D3}";
        private static string FormatDishCode(int dishId) => $"DISH{dishId:D3}";
        private static string FormatUserCode(int userId) => $"USR{userId:D3}";

        // Convert enum status to UI-friendly string
        private static string StatusToUiString(FeedbackStatus status)
        {
            return status switch
            {
                FeedbackStatus.Pending => "pending",
                FeedbackStatus.Reviewed => "reviewed",
                FeedbackStatus.Resolved => "resolved",
                _ => "pending"
            };
        }


        //Get all feedback reports for admin review
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetAllUserFeedbackReports()
        {
            var query = _db.FeedbackReports
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .Include(r => r.Dish)
                .Include(r => r.User)
                .AsQueryable();

            var result = await query
                .OrderByDescending(r => r.SubmittedAt)
                .Select(r => new FeedbackReportListItemDto
                {
                    ReportID = FormatReportCode(r.ReportID),
                    DishName = r.Dish.DishName,
                    UserEmail = r.User.Email,
                    Status = StatusToUiString(r.Status),
                    SubmittedAt = r.SubmittedAt
                })
                .ToListAsync();

            return Ok(result);
        }


        // Get detailed feedback report 
        [Authorize(Roles = "Admin")]
        [HttpGet("{reportId:int}")]
        public async Task<ActionResult> GetFeedbackReportDetails(int reportId)
        {
            var report = await _db.FeedbackReports
                .AsNoTracking()
                .Where(r => !r.IsDeleted && r.ReportID == reportId)
                .Include(r => r.Dish)
                .Include(r => r.User)
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { message = "Feedback report not found." });

            var dto = new FeedbackReportDetailsDto
            {
                ReportID = FormatReportCode(report.ReportID),
                Status = StatusToUiString(report.Status),

                UserEmail = report.User.Email,
                UserID = FormatUserCode(report.UserID),

                DishName = report.Dish.DishName,
                DishID = FormatDishCode(report.DishID),

                SubmittedAt = report.SubmittedAt,
                ReportMessage = report.Message
            };

            return Ok(dto);
        }



        
        [Authorize(Roles = "Admin")]
        [HttpPut("{reportId:int}/status")]
        // Update feedback report status 
        public async Task<ActionResult> UpdateFeedbackReportStatus(
            int reportId,
            [FromBody] UpdateFeedbackStatusRequestDto request)
        {
            var report = await _db.FeedbackReports
                .Include(r => r.User)
                .Include(r => r.Dish)
                .Where(r => !r.IsDeleted && r.ReportID == reportId)
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { message = "Feedback report not found." });

            report.Status = request.Status;
            report.UpdatedAt = DateTime.UtcNow;
            report.UpdatedBy = request.UpdatedBy;

            await _db.SaveChangesAsync();

            if (report.Status == FeedbackStatus.Reviewed ||
                report.Status == FeedbackStatus.Resolved)
            {
                var statusLabel = StatusToUiString(report.Status);
                var statusDisplay = char.ToUpper(statusLabel[0]) + statusLabel[1..];

                await _emailService.SendAsync(
                    report.User.Email,
                    $"Your SafeBite feedback was {statusDisplay}",
                    $@"
        <p>Hello {report.User.FirstName ?? "User"},</p>
        <p>Your feedback report for <b>{report.Dish.DishName}</b> has been marked as <b>{statusDisplay}</b>.</p>
        <p>Thank you for helping us improve SafeBite and keep menu analysis accurate.</p>
        "
                );
            }

            return Ok(new
            {
                reportID = FormatReportCode(report.ReportID),
                status = StatusToUiString(report.Status),
                updatedAt = report.UpdatedAt,
                updatedBy = report.UpdatedBy
            });
        }
    }
}
