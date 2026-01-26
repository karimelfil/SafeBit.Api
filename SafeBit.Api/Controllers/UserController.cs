using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.User;
using System.Security.Claims;

namespace SafeBite.API.Controllers
{
	[ApiController]
	[Route("api/users")]
	[Authorize] // 🔐 JWT REQUIRED FOR ALL ACTIONS
	public class UserController : ControllerBase
	{
		private readonly SafeBiteDbContext _context;

		public UserController(SafeBiteDbContext context)
		{
			_context = context;
		}

		// =====================================================
		// GET USER PERSONAL INFO
		// - Requires JWT authentication
		// - User can only access their own profile
		// =====================================================
		[HttpGet("{userId}/personal-info")]
		public async Task<IActionResult> GetPersonalInfo(int userId)
		{
			var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userIdFromToken == null)
				return Unauthorized("Invalid or missing token.");

			if (int.Parse(userIdFromToken) != userId)
				return Forbid("You are not allowed to view this profile.");

			var user = await _context.Users
				.Where(u => u.UserID == userId && !u.IsDeleted)
				.Select(u => new UserPersonalInfoDto
				{
					FirstName = u.FirstName,
					LastName = u.LastName,
					Email = u.Email,
					Phone = u.Phone,
					DateOfBirth = u.DateOfBirth,
					Gender = u.Gender
				})
				.FirstOrDefaultAsync();

			if (user == null)
				return NotFound("User not found.");

			return Ok(user);
		}

		// =====================================================
		// UPDATE USER PERSONAL INFO
		// - Requires JWT authentication
		// - User can only update their own profile
		// - Only updates provided fields
		// =====================================================
		[HttpPut("{userId}/personal-info")]
		public async Task<IActionResult> UpdatePersonalInfo(
			int userId,
			[FromBody] UserPersonalInfoDto dto)
		{
			var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userIdFromToken == null)
				return Unauthorized("Invalid or missing token.");

			if (int.Parse(userIdFromToken) != userId)
				return Forbid("You are not allowed to update this profile.");

			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted);

			if (user == null)
				return NotFound("User not found.");

			// ✅ Update only provided fields
			if (dto.FirstName != null)
				user.FirstName = dto.FirstName;

			if (dto.LastName != null)
				user.LastName = dto.LastName;

			if (dto.Email != null)
				user.Email = dto.Email;

			if (dto.Phone != null)
				user.Phone = dto.Phone;

			if (dto.DateOfBirth.HasValue)
				user.DateOfBirth = dto.DateOfBirth.Value;

			if (dto.Gender.HasValue)
				user.Gender = dto.Gender.Value;

			await _context.SaveChangesAsync();

			return Ok("Profile updated successfully.");
		}
	}
}
