using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.User;
using SafeBit.Api.Services;
using System.Security.Claims;

namespace SafeBit.Api.Controllers
{
	[ApiController]
	[Route("api/user")]
	// Requires a valid JWT
	public class UserController : ControllerBase
	{
		private readonly SafeBiteDbContext _context;
		private readonly EmailService _emailService;

		public UserController(
			SafeBiteDbContext context,
			EmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		// ================================
		// GET USER PROFILE
		// ================================
		[Authorize]
		[HttpGet("profile/{userId:int}")]
		public async Task<IActionResult> GetUserDetails(int userId)
		{
			var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(currentUserIdStr))
				return Unauthorized("Invalid token.");

			int currentUserId = int.Parse(currentUserIdStr);

			if (currentUserId != userId)
				return Forbid("You can only view your own profile.");

			var user = await _context.Users
				.AsNoTracking()
				.Where(u => u.UserID == userId && !u.IsDeleted)
				.Select(u => new UserPersonalInfoDto
				{
					UserId = u.UserID,
					FirstName = u.FirstName,
					LastName = u.LastName,
					Email = u.Email,
					Phone = u.Phone,
					Gender = u.Gender,
					DateOfBirth = u.DateOfBirth,
					Status = u.Status,
					Registration_Date = u.RegistrationDate
				})
				.FirstOrDefaultAsync();

			if (user == null)
				return NotFound("User not found.");

			return Ok(user);
		}

		// ================================
		// UPDATE USER PERSONAL INFO
		// ================================
		[Authorize]
		[HttpPatch("profile/{userId:int}")]
		public async Task<IActionResult> UpdatePersonalInfo(
			int userId,
			[FromBody] UserPersonalInfoUpdateDto dto)
		{
			if (userId != dto.UserId)
				return BadRequest("User ID mismatch.");

			var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(currentUserIdStr))
				return Unauthorized("Invalid token.");

			int currentUserId = int.Parse(currentUserIdStr);
			if (currentUserId != userId)
				return Forbid("You can only update your own profile.");

			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted);

			if (user == null)
				return NotFound("User not found.");

			// ✅ SAFE PARTIAL UPDATES
			if (dto.FirstName != null)
				user.FirstName = dto.FirstName;

			if (dto.LastName != null)
				user.LastName = dto.LastName;

			if (dto.Phone != null)
				user.Phone = dto.Phone;

			if (dto.DateOfBirth.HasValue)
				user.DateOfBirth = dto.DateOfBirth.Value; // DOB NOT nullable in DB

			if (dto.Gender.HasValue)
				user.Gender = dto.Gender.Value;

			user.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			// 📧 Confirmation email
			await _emailService.SendAsync(
				user.Email,
				"Your SafeBite profile was updated",
				$@"
                <p>Hello {user.FirstName ?? "User"},</p>
                <p>Your <b>personal information was updated successfully</b>.</p>
                <p>If you did not make this change, please contact support immediately.</p>
                <br/>
                <p>– SafeBite Team</p>
                ");

			return Ok("Personal information updated successfully.");
		}
		// ================================
		// GET USER HEALTH INFO
		// ================================
		[Authorize]
		[HttpGet("{userId:int}/health")]
		public async Task<IActionResult> GetUserHealthInfo(int userId)
		{
			var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(currentUserIdStr))
				return Unauthorized("Invalid token.");

			int currentUserId = int.Parse(currentUserIdStr);

			if (currentUserId != userId)
				return Forbid("You can only view your own health information.");

			var userExists = await _context.Users
				.AnyAsync(u => u.UserID == userId && !u.IsDeleted);

			if (!userExists)
				return NotFound("User not found.");

			// ----------------
			// Allergies
			// ----------------
			var allergies = await (
				from ua in _context.UserAllergies
				join a in _context.Allergies on ua.AllergyID equals a.AllergyID
				where ua.UserID == userId
					  && !ua.IsDeleted
					  && !a.IsDeleted
				select new HealthItemDto
				{
					Id = a.AllergyID,
					Name = a.Name,
					Category = a.Category
				}
			).AsNoTracking().ToListAsync();

			// ----------------
			// Diseases
			// ----------------
			var diseases = await (
				from ud in _context.UserDiseases
				join d in _context.Diseases on ud.DiseaseID equals d.DiseaseID
				where ud.UserID == userId
					  && !ud.IsDeleted
					  && !d.IsDeleted
				select new HealthItemDto
				{
					Id = d.DiseaseID,
					Name = d.Name,
					Category = d.Category
				}
			).AsNoTracking().ToListAsync();

			var result = new UserHealthInfoDto
			{
				UserId = userId,
				Allergies = allergies,
				Diseases = diseases
			};

			return Ok(result);
		}
		// ================================
		// GET ALL ALLERGIES
		// ================================
		[HttpGet("allergies")]
		public async Task<IActionResult> GetAllAllergies()
		{
			var allergies = await _context.Allergies
				.AsNoTracking()
				.Where(a => !a.IsDeleted)
				.Select(a => new HealthItemDto
				{
					Id = a.AllergyID,
					Name = a.Name,
					Category = a.Category
				})
				.ToListAsync();

			return Ok(allergies);
		}
		// ================================
		// GET ALL DISEASES
		// ================================
		[HttpGet("diseases")]
		public async Task<IActionResult> GetAllDiseases()
		{
			var diseases = await _context.Diseases
				.AsNoTracking()
				.Where(d => !d.IsDeleted)
				.Select(d => new HealthItemDto
				{
					Id = d.DiseaseID,
					Name = d.Name,
					Category = d.Category
				})
				.ToListAsync();

			return Ok(diseases);
		}
		// ================================
		// UPDATE USER HEALTH PROFILE
		// ================================
		[Authorize]
		[HttpPut("{userId:int}/health")]
		public async Task<IActionResult> UpdateUserHealthProfile(
			int userId,
			[FromBody] UpdateUserHealthProfileDto dto)
		{
			if (userId != dto.UserId)
				return BadRequest("User ID mismatch.");

			var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(currentUserIdStr))
				return Unauthorized("Invalid token.");

			int currentUserId = int.Parse(currentUserIdStr);
			if (currentUserId != userId)
				return Forbid("You can only update your own health profile.");

			var userExists = await _context.Users
				.AnyAsync(u => u.UserID == userId && !u.IsDeleted);

			if (!userExists)
				return NotFound("User not found.");

			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// ----------------------------------
				// SOFT DELETE EXISTING ALLERGIES
				// ----------------------------------
				var existingAllergies = await _context.UserAllergies
					.Where(ua => ua.UserID == userId && !ua.IsDeleted)
					.ToListAsync();

				foreach (var ua in existingAllergies)
				{
					ua.IsDeleted = true;
					ua.UpdatedAt = DateTime.UtcNow;
				}

				// ----------------------------------
				// INSERT NEW ALLERGIES
				// ----------------------------------
				foreach (var allergyId in dto.AllergyIds.Distinct())
				{
					_context.UserAllergies.Add(new Model.UserAllergy
					{
						UserID = userId,
						AllergyID = allergyId,
						CreatedAt = DateTime.UtcNow,
						IsDeleted = false
					});
				}

				// ----------------------------------
				// SOFT DELETE EXISTING DISEASES
				// ----------------------------------
				var existingDiseases = await _context.UserDiseases
					.Where(ud => ud.UserID == userId && !ud.IsDeleted)
					.ToListAsync();

				foreach (var ud in existingDiseases)
				{
					ud.IsDeleted = true;
					ud.UpdatedAt = DateTime.UtcNow;
				}

				// ----------------------------------
				// INSERT NEW DISEASES
				// ----------------------------------
				foreach (var diseaseId in dto.DiseaseIds.Distinct())
				{
					_context.UserDiseases.Add(new Model.UserDisease
					{
						UserID = userId,
						DiseaseID = diseaseId,
						CreatedAt = DateTime.UtcNow,
						IsDeleted = false
					});
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return Ok("Health profile updated successfully.");
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

	}
}
