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


        // Retrieves all allergens from the database.
        [HttpGet("allergens")]
        public async Task<IActionResult> GetAllAllergens()
        {
            var allergens = await _context.Allergies
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Select(a => new AdminAllergenDto
                {
                    Id = a.AllergyID,
                    Name = a.Name,
                    Category = a.Category,
                    Created_At = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new { allergens });
        }

        // Retrieves all diseases from the database.
        [HttpGet("diseases")]
        public async Task<IActionResult> GetAllDiseases()
        {
            var diseases = await _context.Diseases

                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Select(a => new AdminDiseasesDto
                {
                    Id = a.DiseaseID,
                    Name = a.Name,
                    Category = a.Category,
                    Created_At = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new { diseases });
        }

        // Creates a new allergen in the database after validating the input and checking for duplicates.
        [HttpPost("allergens")]
        public async Task<IActionResult> CreateAllergen([FromBody] CreateAllergenDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Allergen name is required.");

            if (string.IsNullOrWhiteSpace(request.Category))
                return BadRequest("Allergen category is required.");

            var exists = await _context.Allergies
                .AnyAsync(a =>
                    a.Name.ToLower() == request.Name.ToLower() &&
                    a.Category.ToLower() == request.Category.ToLower()
                );

            if (exists)
                return Conflict("This allergen already exists.");

            var allergen = new Allergy
            {
                Name = request.Name.Trim(),
                Category = request.Category.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Allergies.Add(allergen);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Allergen created successfully",
                allergen_id = allergen.AllergyID
            });
        }

        // Creates a new disease in the database after validating the input and checking for duplicates.
        [HttpPost("diseases")]
        public async Task<IActionResult> CreateDiseases([FromBody] CreateDiseasesDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Diseases name is required.");

            if (string.IsNullOrWhiteSpace(request.Category))
                return BadRequest("Diseases category is required.");

            var exists = await _context.Diseases
                .AnyAsync(a =>
                    a.Name.ToLower() == request.Name.ToLower() &&
                    a.Category.ToLower() == request.Category.ToLower()
                );

            if (exists)
                return Conflict("This Diseases already exists.");

            var diseases = new Disease
            {
                Name = request.Name.Trim(),
                Category = request.Category.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Diseases.Add(diseases);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "diseases created successfully",
                diseases_id = diseases.DiseaseID
            });
        }


        // Updates an existing allergen's details after validating input and checking for duplicates.
        [HttpPut("allergens/{id}")]
        public async Task<IActionResult> UpdateAllergen(int id,[FromBody] UpdateAllergenDto request)
        {
            var allergen = await _context.Allergies
                .FirstOrDefaultAsync(a => a.AllergyID == id);

            if (allergen == null)
                return NotFound("Allergen not found.");

            var newName = request.Name?.Trim() ?? allergen.Name;
            var newCategory = request.Category?.Trim() ?? allergen.Category;

            var duplicateExists = await _context.Allergies.AnyAsync(a =>
                a.AllergyID != id &&
                a.Name.ToLower() == newName.ToLower() &&
                a.Category.ToLower() == newCategory.ToLower()
            );

            if (duplicateExists)
                return Conflict("Another allergen with the same name and category already exists.");

            allergen.Name = newName;
            allergen.Category = newCategory;
            allergen.UpdatedAt = DateTime.UtcNow;
            allergen.UpdatedBy= "Admin" ;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Allergen updated successfully"
            });
        }




        // Updates an existing disease's details after validating input and checking for duplicates.
        [HttpPut("diseases/{id}")]
        public async Task<IActionResult> UpdateDiseases(int id, [FromBody] UpdateDiseasesDto request)
        {
            var diseases = await _context.Diseases
                .FirstOrDefaultAsync(a => a.DiseaseID == id);

            if (diseases == null)
                return NotFound("Diseases not found.");

            var newName = request.Name?.Trim() ?? diseases.Name;
            var newCategory = request.Category?.Trim() ?? diseases.Category;

            var duplicateExists = await _context.Diseases.AnyAsync(a =>
                a.DiseaseID != id &&
                a.Name.ToLower() == newName.ToLower() &&
                a.Category.ToLower() == newCategory.ToLower()
            );

            if (duplicateExists)
                return Conflict("Another Diseases with the same name and category already exists.");

            diseases.Name = newName;
            diseases.Category = newCategory;
            diseases.UpdatedAt = DateTime.UtcNow;
            diseases.UpdatedBy = "Admin";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Diseases updated successfully"
            });
        }


        // Soft deletes an allergen by setting IsDeleted to true and updating DeletedAt timestamp.
        [HttpDelete("allergens/{id}")]
        public async Task<IActionResult> DeleteAllergen(int id)
        {
            var allergen = await _context.Allergies
                .FirstOrDefaultAsync(a => a.AllergyID == id && !a.IsDeleted);

            if (allergen == null)
                return NotFound("Allergen not found or already deleted.");

            allergen.IsDeleted = true;
            allergen.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Allergen deleted successfully"
            });
        }

        // Soft deletes a disease by setting IsDeleted to true and updating DeletedAt timestamp.
        [HttpDelete("diseases/{id}")]
        public async Task<IActionResult> DeleteDiseases(int id)
        {
            var diseases = await _context.Diseases
                .FirstOrDefaultAsync(a => a.DiseaseID == id && !a.IsDeleted);

            if (diseases == null)
                return NotFound("Diseases not found or already deleted.");

            diseases.IsDeleted = true;
            diseases.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Diseases deleted successfully"
            });
        }

    }
}
