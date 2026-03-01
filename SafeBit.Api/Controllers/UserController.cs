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


        [Authorize(Roles = "User")]
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


        [Authorize(Roles = "User")]
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

	
			if (dto.FirstName != null)
				user.FirstName = dto.FirstName;

			if (dto.LastName != null)
				user.LastName = dto.LastName;

			if (dto.Phone != null)
				user.Phone = dto.Phone;

			if (dto.DateOfBirth.HasValue)
				user.DateOfBirth = dto.DateOfBirth.Value; 

			if (dto.Gender.HasValue)
				user.Gender = dto.Gender.Value;

			user.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();


			await _emailService.SendAsync(
				user.Email,
				"Your SafeBite profile was updated",
				$@"
                <p>Hello {user.FirstName ?? "User"},</p>
                <p>Your <b>personal information was updated successfully</b>.</p>
                <p>If you did not make this change, please contact support immediately.</p>
                ");

			return Ok("Personal information updated successfully.");
		}

        [Authorize(Roles = "User")]
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
        [Authorize(Roles = "User")]
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

            var userExists = await _context.Users.AnyAsync(u => u.UserID == userId && !u.IsDeleted);
            if (!userExists)
                return NotFound("User not found.");

            var incomingAllergyIds = (dto.AllergyIds ?? new List<int>()).Distinct().ToList();
            var incomingDiseaseIds = (dto.DiseaseIds ?? new List<int>()).Distinct().ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                var allUserAllergies = await _context.UserAllergies
                    .Where(x => x.UserID == userId)
                    .ToListAsync();

     
                foreach (var ua in allUserAllergies.Where(x => !x.IsDeleted && !incomingAllergyIds.Contains(x.AllergyID)))
                {
                    ua.IsDeleted = true;
                    ua.UpdatedAt = DateTime.UtcNow;
                }

  
                foreach (var allergyId in incomingAllergyIds)
                {
                    var existing = allUserAllergies.FirstOrDefault(x => x.AllergyID == allergyId);
                    if (existing != null)
                    {
                        if (existing.IsDeleted)
                        {
                            existing.IsDeleted = false;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        _context.UserAllergies.Add(new Model.UserAllergy
                        {
                            UserID = userId,
                            AllergyID = allergyId,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                }


                var allUserDiseases = await _context.UserDiseases
                    .Where(x => x.UserID == userId)
                    .ToListAsync();

                foreach (var ud in allUserDiseases.Where(x => !x.IsDeleted && !incomingDiseaseIds.Contains(x.DiseaseID)))
                {
                    ud.IsDeleted = true;
                    ud.UpdatedAt = DateTime.UtcNow;
                }

                foreach (var diseaseId in incomingDiseaseIds)
                {
                    var existing = allUserDiseases.FirstOrDefault(x => x.DiseaseID == diseaseId);
                    if (existing != null)
                    {
                        if (existing.IsDeleted)
                        {
                            existing.IsDeleted = false;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        _context.UserDiseases.Add(new Model.UserDisease
                        {
                            UserID = userId,
                            DiseaseID = diseaseId,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
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

        [Authorize(Roles = "User")]
        [HttpPost("{userId:int}/health/allergies")]
        public async Task<IActionResult> AddAllergies(int userId, [FromBody] AddUserAllergiesDto dto)
        {
            if (userId != dto.UserId) return BadRequest("User ID mismatch.");

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdStr)) return Unauthorized("Invalid token.");
            if (int.Parse(currentUserIdStr) != userId) return Forbid();

            var ids = (dto.AllergyIds ?? new List<int>()).Distinct().ToList();
            if (ids.Count == 0) return Ok("Nothing to add.");

            var existing = await _context.UserAllergies
                .Where(x => x.UserID == userId && ids.Contains(x.AllergyID))
                .ToListAsync();

            foreach (var allergyId in ids)
            {
                var row = existing.FirstOrDefault(x => x.AllergyID == allergyId);
                if (row != null)
                {
                    if (row.IsDeleted)
                    {
                        row.IsDeleted = false;
                        row.UpdatedAt = DateTime.UtcNow;
                    }
                
                }
                else
                {
                    _context.UserAllergies.Add(new Model.UserAllergy
                    {
                        UserID = userId,
                        AllergyID = allergyId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Allergies added.");
        }

        [Authorize(Roles = "User")]
        [HttpDelete("{userId:int}/health/allergies/{allergyId:int}")]
        public async Task<IActionResult> DeleteAllergy(int userId, int allergyId)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdStr)) return Unauthorized("Invalid token.");
            if (int.Parse(currentUserIdStr) != userId) return Forbid();

            var row = await _context.UserAllergies
                .FirstOrDefaultAsync(x => x.UserID == userId && x.AllergyID == allergyId && !x.IsDeleted);

            if (row == null) return NotFound("Allergy not found.");

            row.IsDeleted = true;
            row.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Allergy deleted.");
        }

        [Authorize(Roles = "User")]
        [HttpPost("{userId:int}/health/diseases")]
        public async Task<IActionResult> AddDiseases(int userId, [FromBody] AddUserDiseasesDto dto)
        {
            if (userId != dto.UserId) return BadRequest("User ID mismatch.");

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdStr)) return Unauthorized("Invalid token.");
            if (int.Parse(currentUserIdStr) != userId) return Forbid();

            var ids = (dto.DiseaseIds ?? new List<int>()).Distinct().ToList();
            if (ids.Count == 0) return Ok("Nothing to add.");

            var existing = await _context.UserDiseases
                .Where(x => x.UserID == userId && ids.Contains(x.DiseaseID))
                .ToListAsync();

            foreach (var diseaseId in ids)
            {
                var row = existing.FirstOrDefault(x => x.DiseaseID == diseaseId);
                if (row != null)
                {
                    if (row.IsDeleted)
                    {
                        row.IsDeleted = false;
                        row.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    _context.UserDiseases.Add(new Model.UserDisease
                    {
                        UserID = userId,
                        DiseaseID = diseaseId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Diseases added.");
        }

        [Authorize(Roles = "User")]
        [HttpDelete("{userId:int}/health/diseases/{diseaseId:int}")]
        public async Task<IActionResult> DeleteDisease(int userId, int diseaseId)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdStr)) return Unauthorized("Invalid token.");
            if (int.Parse(currentUserIdStr) != userId) return Forbid();

            var row = await _context.UserDiseases
                .FirstOrDefaultAsync(x => x.UserID == userId && x.DiseaseID == diseaseId && !x.IsDeleted);

            if (row == null) return NotFound("Disease not found.");

            row.IsDeleted = true;
            row.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Disease deleted.");
        }



        [Authorize(Roles = "User")]
        [HttpGet("{userId:int}/health/summary")]
        public async Task<IActionResult> GetUserHealthSummary(int userId)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdStr))
                return Unauthorized("Invalid token.");

            if (int.Parse(currentUserIdStr) != userId)
                return Forbid("You can only view your own health information.");

            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == userId && !u.IsDeleted);

            if (!userExists)
                return NotFound("User not found.");

            var allergies = await (
                from ua in _context.UserAllergies
                join a in _context.Allergies on ua.AllergyID equals a.AllergyID
                where ua.UserID == userId
                      && !ua.IsDeleted
                      && !a.IsDeleted
                select a.Name
            )
            .AsNoTracking()
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

            var diseases = await (
                from ud in _context.UserDiseases
                join d in _context.Diseases on ud.DiseaseID equals d.DiseaseID
                where ud.UserID == userId
                      && !ud.IsDeleted
                      && !d.IsDeleted
                select d.Name
            )
            .AsNoTracking()
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

            return Ok(new UserHealthSummaryDto
            {
                UserId = userId,
                Allergies = allergies,
                Diseases = diseases
            });
        }


    }
}
