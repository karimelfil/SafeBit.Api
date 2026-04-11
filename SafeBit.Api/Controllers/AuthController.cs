using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs;
using SafeBit.Api.DTOs.Register;
using SafeBit.Api.Model;
using SafeBit.Api.Model.Enums;
using SafeBit.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SafeBite.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SafeBiteDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AuthController(SafeBiteDbContext context, IConfiguration configuration, EmailService emailService    )
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }


        // Registers a new user with the RegisterRequest dto
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
    
            if (request.Password != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

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
                    RoleID = 2, // Default to User role
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
                    Status = "Active", // Default status
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                
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

        
        // Login a user with the LoginRequest dto and return a JWT token 
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

        
        // Forgot password genearte a secure token and send reset link via email 
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == request.Email &&
                    !u.IsDeleted);

            if (user == null)
                return Ok("Email does not exist.");

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));// Generate a secure random token
            var hashedToken = BCrypt.Net.BCrypt.HashPassword(rawToken);// Hash the token before storing

            user.PasswordResetToken = hashedToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetLink =
                $"http://192.168.18.10:5173/reset-password?token={Uri.EscapeDataString(rawToken)}";

            await _emailService.SendAsync(
                user.Email,
                "Reset your SafeBite password",
                $@"
        <p>Hello {user.FirstName ?? "User"},</p>
        <p>We received a request to reset your password.</p>
        <p>
            <a href='{resetLink}' style='display:inline-block;background:#0f766e;color:#ffffff;padding:10px 18px;border-radius:8px;text-decoration:none;font-weight:600;'>
                Reset Password
            </a>
        </p>
        <p>This secure link expires in 1 hour.</p>
        <p>If you did not request this, you can safely ignore this message.</p>
        "
            );

            return Ok("If the email exists, a reset link has been sent.");
        }



        // Reset password using the token from the forgot password  and update  user password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            var users = await _context.Users
                .Where(u => u.PasswordResetToken != null &&
                            u.PasswordResetTokenExpiry > DateTime.UtcNow)
                .ToListAsync();

            var user = users.FirstOrDefault(u =>
                BCrypt.Net.BCrypt.Verify(request.Token, u.PasswordResetToken));

            if (user == null)
                return BadRequest("Invalid or expired token.");

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            if (request.NewPassword.Length < 8)
                return BadRequest("Password must be at least 8 characters.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);


            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully.");
        }




        // Soft deactivate the user acccount 
        [Authorize(Roles = "User")]
        [HttpPost("deactivate-account")]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);// Get the user ID from the JWT claims (logged-in user)

            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserID == userId &&
                    !u.IsDeleted &&
                    !u.IsSuspended);

            if (user == null)
                return BadRequest("User not found or already deactivated.");

            user.IsSuspended = true;// Mark the account as suspended
            user.Status = "Deactivated";// Update the status to "Deactivated"
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailService.SendAsync(
                user.Email,
                "Your SafeBite account has been deactivated",
                $@"
            <p>Hello {user.FirstName ?? "User"},</p>
            <p>Your SafeBite account has been successfully <b>deactivated</b>.</p>
            <p>If this was not intended, contact our support team for assistance.</p>
        "
            );

            return Ok("Account deactivated successfully.");
        }


        // Logout the user 
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);//read user ID from JWT claims 
            var tokenJti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);//read the jti claim from JWT claims

            if (userId == null || tokenJti == null)
                return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == int.Parse(userId));

            if (user == null)
                return Unauthorized();


            user.ActiveJti = null;
            user.LastLogoutAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Logged out successfully.");
        }




        // Generates a JWT token 
    private string GenerateJwtToken(User user, string roleType)
{
    var jwtSettings = _configuration.GetSection("Jwt");// Get JWT settings from configuration
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
    );
            var jti = Guid.NewGuid().ToString();

            user.ActiveJti = jti;
            _context.SaveChanges();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, roleType),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
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
