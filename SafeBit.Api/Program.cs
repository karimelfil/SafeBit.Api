using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SafeBit.Api.Data;
using SafeBit.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

// ====================== CORS ======================
const string CorsPolicy = "_allowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(
                "http://localhost:5173",     // Vite dev
                "http://192.168.18.10:5173"  // same network access
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// ====================== JWT CONFIG ======================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // for dev only
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // tokens expire exactly at expiration
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
app.UseCors(CorsPolicy);

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
app.Run();