using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.User;

namespace SafeBite.API.Controllers
{
	[ApiController]
	[Route("api/users")]
	public class UserController : ControllerBase
	{
		private readonly SafeBiteDbContext _context;

		public UserController(SafeBiteDbContext context)
		{
			_context = context;
		}

		// ============================
		// GET USER PERSONAL INFO 
		// ============================
		[HttpGet("{userId}/personal-info")]
		public async Task<IActionResult> GetPersonalInfo(int userId)
		{
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
	}
}
