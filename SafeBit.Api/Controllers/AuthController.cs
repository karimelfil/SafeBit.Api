using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs;
using SafeBit.Api.DTOs.Register;
using SafeBit.Api.Model;
using SafeBit.Api.Model.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net; 

namespace SafeBite.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SafeBiteDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(SafeBiteDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
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

        // Authenticates a user, validates credentials, and returns a JWT token on success.
        // Verifies user credentials, checks account status, and issues a JWT token.

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == request.Email &&
                    !u.IsDeleted &&
                    !u.IsSuspended);

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var roleType = await _context.Roles
                .Where(r => r.RoleID == user.RoleID)
                .Select(r => r.Type)
                .FirstOrDefaultAsync();

            if (roleType == null)
                return Unauthorized("User role not found.");

            var token = GenerateJwtToken(user, roleType);

            return Ok(new
            {
                token,
                role = roleType,
                userId = user.UserID
            });
        }









        // JWT TOKEN GENERATION
        private string GenerateJwtToken(User user, string roleType)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            );

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, roleType),
        new Claim("role", roleType)
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwtSettings["DurationInMinutes"]!)
                ),
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
