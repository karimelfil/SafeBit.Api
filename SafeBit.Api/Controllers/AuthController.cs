using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Register;
using SafeBit.Api.Model;
using SafeBit.Api.Model.Enums;
using System.Security.Cryptography;
using System.Text;

namespace SafeBite.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SafeBiteDbContext _context;

        public AuthController(SafeBiteDbContext context)
        {
            _context = context;
        }

        // Registers a new user by validating input, ensuring email uniqueness,
        // creating the user account, and linking selected allergies and diseases
        // within a single database transaction.
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
    
            if (request.Password != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            // Pregnancy rule
            if (request.Gender == Gender.Male && request.IsPregnant)
                return BadRequest("Male users cannot be pregnant.");

 
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (emailExists)
                return BadRequest("Email already exists.");

       
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                var user = new User
                {
                    RoleID = 2, // User role
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    IsPregnant = request.Gender == Gender.Female && request.IsPregnant,
                    IsSuspended = false,
                    PasswordHash = HashPassword(request.Password),
                    RegistrationDate = DateTime.UtcNow,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                //  Add allergies
                foreach (var allergyId in request.AllergyIds)
                {
                    _context.UserAllergies.Add(new UserAllergy
                    {
                        UserID = user.UserID,
                        AllergyID = allergyId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }

                //  Add diseases
                foreach (var diseaseId in request.DiseaseIds)
                {
                    _context.UserDiseases.Add(new UserDisease
                    {
                        UserID = user.UserID,
                        DiseaseID = diseaseId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("User registered successfully.");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred during registration.");
            }
        }


        // PASSWORD HASHING
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
