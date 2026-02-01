using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SafeBit.Api.Data;
using SafeBit.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================== DATABASE ======================
builder.Services.AddDbContext<SafeBiteDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		npgsql =>
		{
			npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "safebit");
		}
	)
);

// ====================== JWT CONFIG ======================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,

			ValidIssuer = jwtSettings["Issuer"],
			ValidAudience = jwtSettings["Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(key),

			RoleClaimType = ClaimTypes.Role,
			NameClaimType = ClaimTypes.NameIdentifier
		};

		// ====================== TOKEN VALIDATION EVENTS ======================
		options.Events = new JwtBearerEvents
		{
			OnTokenValidated = async context =>
			{
				var db = context.HttpContext.RequestServices
					.GetRequiredService<SafeBiteDbContext>();

				var userId = context.Principal?
					.FindFirstValue(ClaimTypes.NameIdentifier);

				var jti = context.Principal?
					.FindFirstValue(JwtRegisteredClaimNames.Jti);

				if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(jti))
				{
					context.Fail("Invalid token");
					return;
				}

				var user = await db.Users.FindAsync(int.Parse(userId));

				// Token revoked or user deleted/suspended
				if (user == null || user.ActiveJti != jti || user.IsDeleted || user.IsSuspended)
				{
					context.Fail("Token revoked");
				}
			}
		};
	});


// ====================== AUTHORIZATION ======================
builder.Services.AddAuthorization();

// ====================== SERVICES ======================
builder.Services.AddScoped<EmailService>();
builder.Services.AddControllers();

// ====================== SWAGGER WITH JWT ======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter: Bearer {your JWT token}"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

var app = builder.Build();

// ====================== MIDDLEWARE ======================
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();