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

        /// Generates a secure password reset token, stores it with an expiration time,
        /// and sends a password reset link to the user's email if the account exists.
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == request.Email &&
                    !u.IsDeleted);

            if (user == null)
                return Ok("If the email exists, a reset link has been sent.");

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedToken = BCrypt.Net.BCrypt.HashPassword(rawToken);

            user.PasswordResetToken = hashedToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:3000/reset-password?token={Uri.EscapeDataString(rawToken)}";

            await _emailService.SendAsync(
                user.Email,
                "Reset your SafeBite password",
                $"<p>Click below to reset your password:</p><a href='{resetLink}'>Reset Password</a>"
            );

            return Ok("If the email exists, a reset link has been sent.");
        }


        /// Resets a user's password using a valid, unexpired password reset token.
        /// Contains the reset token, new password, and confirmation password.
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var decodedToken = WebUtility.UrlDecode(request.Token);

            var users = await _context.Users
                .Where(u =>
                    u.PasswordResetToken != null &&
                    u.PasswordResetTokenExpiry > DateTime.UtcNow &&
                    !u.IsDeleted)
                .ToListAsync();

            var user = users.FirstOrDefault(u =>
                BCrypt.Net.BCrypt.Verify(decodedToken, u.PasswordResetToken!));

            if (user == null)
                return BadRequest("Invalid or expired token.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }


        /// The user is identified from the JWT token (NameIdentifier claim).
        /// Once deactivated, the account is marked as suspended and cannot be used to log in.
        /// A confirmation email is sent after successful deactivation.
        [Authorize(Roles = "User")]
        [HttpPost("deactivate-account")]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

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

            user.IsSuspended = true;
            user.Status = "Deactivated";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailService.SendAsync(
                user.Email,
                "Your SafeBite account has been deactivated",
                $@"
            <p>Hello {user.FirstName ?? "User"},</p>
            <p>Your SafeBite account has been successfully <b>deactivated</b>.</p>
            <p>If this was a mistake, please contact support.</p>
            <br/>
            <p>– SafeBite Team</p>
        "
            );

            return Ok("Account deactivated successfully.");
        }


        // Logs out the authenticated user by revoking the active JWT token and recording the logout time.

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenJti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

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




        // Generates a JWT token for the authenticated user with relevant claims.
        private string GenerateJwtToken(User user, string roleType)
{
    var jwtSettings = _configuration.GetSection("Jwt");
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
