using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.HealthAnalytics;
using System.Globalization;

namespace SafeBit.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class HealthAnalyticsController : ControllerBase
    {
        private readonly SafeBiteDbContext _context;

        public HealthAnalyticsController(SafeBiteDbContext context)
        {
            _context = context;
        }
        // Anonymized health statistics 
        [HttpGet]
        public async Task<ActionResult> GetAnonymizedHealthStatistics()
        {
            var totalUsers = await _context.Users.AsNoTracking().CountAsync();

            var usersWithAllergies = await _context.UserAllergies
                .AsNoTracking()
                .Select(x => x.UserID)
                .Distinct()
                .CountAsync();

            var usersWithDiseases = await _context.UserDiseases
                .AsNoTracking()
                .Select(x => x.UserID)
                .Distinct()
                .CountAsync();

            double allergiesPercent = totalUsers == 0 ? 0 : Math.Round((usersWithAllergies * 100.0) / totalUsers, 1);
            double diseasesPercent = totalUsers == 0 ? 0 : Math.Round((usersWithDiseases * 100.0) / totalUsers, 1);

            var nowUtc = DateTime.UtcNow;
            var startThisMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startPrevMonth = startThisMonth.AddMonths(-1);

            var newUsersThisMonth = await _context.Users
                .AsNoTracking()
                .CountAsync(u => EF.Property<DateTime>(u, "CreatedAt") >= startThisMonth);

            var newUsersPrevMonth = await _context.Users
                .AsNoTracking()
                .CountAsync(u =>
                    EF.Property<DateTime>(u, "CreatedAt") >= startPrevMonth &&
                    EF.Property<DateTime>(u, "CreatedAt") < startThisMonth);

            double monthlyGrowthPercent =
                newUsersPrevMonth == 0
                    ? (newUsersThisMonth > 0 ? 100.0 : 0.0)
                    : Math.Round(((newUsersThisMonth - newUsersPrevMonth) * 100.0) / newUsersPrevMonth, 1);

            var allergyCounts = await _context.UserAllergies
                .AsNoTracking()
                .GroupBy(x => x.AllergyID)
                .Select(g => new
                {
                    AllergyID = g.Key,
                    AffectedUsers = g.Select(x => x.UserID).Distinct().Count()
                })
                .ToListAsync();

            var allergyNames = await _context.Allergies
                .AsNoTracking()
                .Select(a => new { a.AllergyID, a.Name })
                .ToListAsync();

            var detailedAllergyStats = (
                from c in allergyCounts
                join n in allergyNames on c.AllergyID equals n.AllergyID
                orderby c.AffectedUsers descending
                select new AllergyStatDto
                {
                    AllergyID = c.AllergyID,
                    Name = n.Name,
                    AffectedUsers = c.AffectedUsers,
                    PercentOfTotalUsers = totalUsers == 0 ? 0 : Math.Round((c.AffectedUsers * 100.0) / totalUsers, 1)
                }
            ).ToList();

            var diseaseCounts = await _context.UserDiseases
                .AsNoTracking()
                .GroupBy(x => x.DiseaseID)
                .Select(g => new
                {
                    DiseaseID = g.Key,
                    AffectedUsers = g.Select(x => x.UserID).Distinct().Count()
                })
                .ToListAsync();

            var diseaseNames = await _context.Diseases
                .AsNoTracking()
                .Select(d => new { d.DiseaseID, d.Name })
                .ToListAsync();

            var allDiseaseStats = (
                from c in diseaseCounts
                join n in diseaseNames on c.DiseaseID equals n.DiseaseID
                orderby c.AffectedUsers descending
                select new DiseaseStatDto
                {
                    DiseaseID = c.DiseaseID,
                    Name = n.Name,
                    AffectedUsers = c.AffectedUsers,
                    PercentOfTotalUsers = totalUsers == 0 ? 0 : Math.Round((c.AffectedUsers * 100.0) / totalUsers, 1)
                }
            ).ToList();

            var topDiseases = allDiseaseStats.Take(2).ToList();
            var otherDiseases = allDiseaseStats.Skip(2).ToList();

            if (otherDiseases.Count > 0)
            {
                var otherUsers = otherDiseases.Sum(x => x.AffectedUsers);
                topDiseases.Add(new DiseaseStatDto
                {
                    DiseaseID = 0,
                    Name = "Other",
                    AffectedUsers = otherUsers,
                    PercentOfTotalUsers = totalUsers == 0 ? 0 : Math.Round((otherUsers * 100.0) / totalUsers, 1)
                });
            }

            var months = new List<(DateTime start, DateTime end, string label)>();
            for (int i = 5; i >= 0; i--)
            {
                var start = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                var end = start.AddMonths(1);
                months.Add((start, end, start.ToString("MMM", CultureInfo.InvariantCulture)));
            }

            var trends = new List<MonthlyTrendDto>();
            foreach (var month in months)
            {
                var allergiesMonthly = await _context.UserAllergies
                    .AsNoTracking()
                    .Where(x =>
                        EF.Property<DateTime>(x, "CreatedAt") >= month.start &&
                        EF.Property<DateTime>(x, "CreatedAt") < month.end)
                    .Select(x => x.UserID)
                    .Distinct()
                    .CountAsync();

                var diseasesMonthly = await _context.UserDiseases
                    .AsNoTracking()
                    .Where(x =>
                        EF.Property<DateTime>(x, "CreatedAt") >= month.start &&
                        EF.Property<DateTime>(x, "CreatedAt") < month.end)
                    .Select(x => x.UserID)
                    .Distinct()
                    .CountAsync();

                trends.Add(new MonthlyTrendDto
                {
                    Month = month.label,
                    Allergies = allergiesMonthly,
                    Diseases = diseasesMonthly
                });
            }
            
            // Generate key insights based on the collected data
            var keyInsights = new List<KeyInsightDto>
            {
                new KeyInsightDto
                {
                    Title = "High Allergy Prevalence",
                    Message = $"{allergiesPercent}% of users report at least one allergy, highlighting the need for accurate allergen detection.",
                    Type = "primary"
                },
                new KeyInsightDto
                {
                    Title = "Chronic Disease Management",
                    Message = $"{diseasesPercent}% of users manage chronic conditions, emphasizing the importance of nutritional tracking and planning.",
                    Type = "info"
                },
                new KeyInsightDto
                {
                    Title = "Growing Health Awareness",
                    Message = $"{monthlyGrowthPercent}% monthly growth indicates increasing user engagement with health tracking features.",
                    Type = "success"
                }
            };

            if (detailedAllergyStats.Count > 0)
            {
                var topAllergy = detailedAllergyStats.First();
                keyInsights.Insert(1, new KeyInsightDto
                {
                    Title = "Most Common Allergy",
                    Message = $"{topAllergy.Name} affects {topAllergy.PercentOfTotalUsers}% of users, making it the most common allergy in the system.",
                    Type = "warning"
                });
            }

            var response = new HealthAnalyticsResponseDto
            {
                TotalUsers = totalUsers,
                UsersWithAllergies = usersWithAllergies,
                UsersWithAllergiesPercent = allergiesPercent,
                UsersWithDiseases = usersWithDiseases,
                UsersWithDiseasesPercent = diseasesPercent,
                MonthlyGrowthPercent = monthlyGrowthPercent,
                DetailedAllergyStatistics = detailedAllergyStats,
                DiseaseDistribution = topDiseases,
                HealthDataTrends = trends,
                KeyInsights = keyInsights
            };

            return Ok(response);
        }
    }
}
