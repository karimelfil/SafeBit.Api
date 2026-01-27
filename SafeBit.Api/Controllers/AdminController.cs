using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Admin;
using SafeBit.Api.Model;
using SafeBit.Api.Services;

namespace SafeBit.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly SafeBiteDbContext _context;
        private readonly EmailService _emailService;

        public AdminController(SafeBiteDbContext context, EmailService emailService )
        {
            _context = context;
            _emailService = emailService;
        }

        // Retrieves all non-deleted users with role type "User" and orders them by registration date.
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Join(
                    _context.Roles,
                    u => u.RoleID,
                    r => r.RoleID,
                    (u, r) => new { User = u, Role = r }
                )
                .Where(x =>
                    !x.User.IsDeleted &&
                    x.Role.Type == "User"   
                )
                .OrderBy(x => x.User.RegistrationDate)
                .Select(x => new AdminUserListDto
                {
                    User_Id = x.User.UserID,
                    Username =
                        (x.User.FirstName ?? "") + " " + (x.User.LastName ?? ""),
                    Email = x.User.Email,
                    Status = x.User.Status,
                    Registration_Date = x.User.RegistrationDate
                })
                .ToListAsync();

            return Ok(new { users });
        }


        // Retrieves detailed information about a specific user by their ID.
        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Join(
                    _context.Roles,
                    u => u.RoleID,
                    r => r.RoleID,
                    (u, r) => new { User = u, Role = r }
                )
                .Where(x =>
                    x.User.UserID == userId &&
                    !x.User.IsDeleted &&
                    x.Role.Type == "User"   
                )
                .Select(x => new AdminUserDetailsDto
                {
                    User_Id = x.User.UserID,
                    Email = x.User.Email,
                    First_Name = x.User.FirstName,
                    Last_Name = x.User.LastName,
                    Phone = x.User.Phone,
                    Status = x.User.Status,
                    Date_Of_Birth = x.User.DateOfBirth,
                    Gender = x.User.Gender.ToString(),
                    Registration_Date = x.User.RegistrationDate
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(new { user });
        }


        // Updates user information based on provided data.
        [HttpPut("users/{userId:int}")]
        public async Task<IActionResult> UpdateUser(
     int userId,
     [FromBody] AdminUpdateUserDto updateData)
        {
            var result = await _context.Users
                .Join(
                    _context.Roles,
                    u => u.RoleID,
                    r => r.RoleID,
                    (u, r) => new { User = u, Role = r }
                )
                .Where(x =>
                    x.User.UserID == userId &&
                    !x.User.IsDeleted &&
                    x.Role.Type == "User"
                )
                .Select(x => x.User)
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound("User not found.");

            var user = result;

  
            if (!string.IsNullOrWhiteSpace(updateData.Email))
            {
                bool emailExists = await _context.Users.AnyAsync(u =>
                    u.Email == updateData.Email &&
                    u.UserID != userId &&
                    !u.IsDeleted
                );

                if (emailExists)
                    return BadRequest("Email already in use.");
            }

        
            if (updateData.First_Name != null)
                user.FirstName = updateData.First_Name;

            if (updateData.Last_Name != null)
                user.LastName = updateData.Last_Name;

            if (updateData.Email != null)
                user.Email = updateData.Email;

            if (updateData.Phone != null)
                user.Phone = updateData.Phone;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("User information updated successfully.");
        }

        // Suspends a user account by setting IsSuspended to true and updating the status.
        [HttpPost("users/{userId:int}/suspend")]
        public async Task<IActionResult> SuspendUser(int userId)
        {
 
            var result = await _context.Users
                .Join(
                    _context.Roles,
                    u => u.RoleID,
                    r => r.RoleID,
                    (u, r) => new { User = u, Role = r }
                )
                .Where(x =>
                    x.User.UserID == userId &&
                    !x.User.IsDeleted &&
                    x.Role.Type == "User"   
                )
                .Select(x => x.User)
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound("User not found.");

            var user = result; 

            if (user.IsSuspended)
                return BadRequest("User account is already suspended.");

      
            user.IsSuspended = true;
            user.Status = "Suspended";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailService.SendAsync(
                user.Email,
                "Your SafeBite account has been suspended",
                $@"
        <p>Hello {user.FirstName ?? "User"},</p>

        <p>Your <b>SafeBite account has been suspended</b> by an administrator.</p>

        <br/>
        <p>– SafeBite Team</p>
        "
            );

            return Ok("User account suspended successfully.");
        }

        // Reactivates a suspended user account by setting IsSuspended to false and updating the status.
        [HttpPost("users/{userId:int}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int userId)
        {
            var result = await _context.Users
                .Join(
                    _context.Roles,
                    u => u.RoleID,
                    r => r.RoleID,
                    (u, r) => new { User = u, Role = r }
                )
                .Where(x =>
                    x.User.UserID == userId &&
                    !x.User.IsDeleted &&
                    x.Role.Type == "User"   
                )
                .Select(x => x.User)
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound("User not found.");

            var user = result; 

            if (!user.IsSuspended)
                return BadRequest("User account is already active.");

            user.IsSuspended = false;
            user.Status = "Active";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailService.SendAsync(
                user.Email,
                "Your SafeBite account has been reactivated",
                $@"
        <p>Hello {user.FirstName ?? "User"},</p>

        <p>Good news 🎉</p>

        <p>Your <b>SafeBite account has been reactivated</b>. You can now log in and continue using the platform.</p>

        <br/>
        <p>– SafeBite Team</p>
        "
            );

            return Ok("User account reactivated successfully.");
        }



    }
}
