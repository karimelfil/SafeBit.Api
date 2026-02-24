using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using SafeBit.Api.Data;
using SafeBit.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


ExcelPackage.License.SetNonCommercialOrganization("SafeBit");


builder.Services.AddDbContext<SafeBiteDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "safebit");
        }
    )
);


const string CorsPolicy = "_allowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://172.22.128.1:5173",
                "http://192.168.56.1:5173",
                "http://192.168.18.10:5173",
                "https://5mkn7tb3-5173.euw.devtunnels.ms"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});


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
        options.RequireHttpsMetadata = false;
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




builder.Services.AddAuthorization();


builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<MenuAnalysisService>();
builder.Services.AddHttpClient<AiAgentService>();
builder.Services.AddControllers();


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

var emailAssetsPath = Path.Combine(builder.Environment.ContentRootPath, "Assets", "Email");
Directory.CreateDirectory(emailAssetsPath);


app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(emailAssetsPath),
        RequestPath = "/email-assets"
    }
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
