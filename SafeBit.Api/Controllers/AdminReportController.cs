using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.DTOs.Report;
using SafeBit.Api.Services;
using System.Text;

namespace SafeBit.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminReportController : ControllerBase

    {
        private readonly SafeBiteDbContext _context;

        public AdminReportController(SafeBiteDbContext context)
        {
            _context = context;
        }

        //this endpoint generates various analytics reports based on the specified type and date range
        [HttpPost("generate-analytics-report")]
        public async Task<IActionResult> GenerateAnalyticsReport(GenerateReportRequestDto request)
        {
            DateTime fromDate;
            // Determine the starting date based on the requested date range
            switch (request.DateRange)
            {
                case "Last7Days":
                    fromDate = DateTime.UtcNow.AddDays(-7);
                    break;
                case "Last30Days":
                    fromDate = DateTime.UtcNow.AddDays(-30);
                    break;
                case "Last90Days":
                    fromDate = DateTime.UtcNow.AddDays(-90);
                    break;
                case "LastYear":
                    fromDate = DateTime.UtcNow.AddYears(-1);
                    break;
                case "AllTime":
                    fromDate = DateTime.MinValue;
                    break;
                default:
                    fromDate = DateTime.UtcNow.AddDays(-30);
                    break;
            }

            var response = new GenerateReportResponseDto
            {
                ReportType = request.ReportType,
                DateRange = request.DateRange,
                GeneratedAt = DateTime.UtcNow
            };

            //user demographics report groups
            if (request.ReportType == "UserDemographics")
            {
                var users = await _context.Users
                    .Where(u => !u.IsDeleted && u.CreatedAt >= fromDate)
                    .ToListAsync();

                var total = users.Count;
                response.TotalRecords = total;

                var groups = new Dictionary<string, int>
        {
            { "Age 18-25", 0 },
            { "Age 26-35", 0 },
            { "Age 36-45", 0 },
            { "Age 46-60", 0 },
            { "Age 60+", 0 }
        };

                foreach (var user in users)
                {
                    var age = DateTime.UtcNow.Year - user.DateOfBirth.Year;
                    if (user.DateOfBirth.Date > DateTime.UtcNow.AddYears(-age).Date)
                    {
                        age--;
                    }

                    if (age >= 18 && age <= 25) groups["Age 18-25"]++;
                    else if (age <= 35) groups["Age 26-35"]++;
                    else if (age <= 45) groups["Age 36-45"]++;
                    else if (age <= 60) groups["Age 46-60"]++;
                    else groups["Age 60+"]++;
                }

                response.Data = groups.Select(g => new ReportItemDto
                {
                    Category = g.Key,
                    Count = g.Value,
                    Percentage = total == 0 ? 0 : Math.Round((double)g.Value / total * 100, 2)
                }).ToList();
            }


            //allergy statistics report groups
            else if (request.ReportType == "AllergyStatistics")
            {
                var allergyNames = await _context.UserAllergies
                    .Where(ua => !ua.IsDeleted && ua.CreatedAt >= fromDate)
                    .Join(
                        _context.Allergies.Where(a => !a.IsDeleted),
                        ua => ua.AllergyID,
                        a => a.AllergyID,
                        (_, a) => a.Name)
                    .ToListAsync();

                var total = allergyNames.Count;
                response.TotalRecords = total;

                response.Data = allergyNames
                    .GroupBy(name => name)
                    .Select(g => new ReportItemDto
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
            }


            //disease statistics report groups
            else if (request.ReportType == "DiseaseStatistics")
            {
                var diseaseNames = await _context.UserDiseases
                    .Where(ud => !ud.IsDeleted && ud.CreatedAt >= fromDate)
                    .Join(
                        _context.Diseases.Where(d => !d.IsDeleted),
                        ud => ud.DiseaseID,
                        d => d.DiseaseID,
                        (_, d) => d.Name)
                    .ToListAsync();

                var total = diseaseNames.Count;
                response.TotalRecords = total;

                response.Data = diseaseNames
                    .GroupBy(name => name)
                    .Select(g => new ReportItemDto
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
            }


            //most common allergens in dishes report groups
            else if (request.ReportType == "MostCommonAllergensInDishes")
            {
                var knownAllergens = (await _context.Allergies
                        .Where(a => !a.IsDeleted)
                        .Select(a => a.Name.ToLower())
                        .ToListAsync())
                    .ToHashSet();

                var ingredientNames = await _context.DishIngredients
                    .Where(di => !di.IsDeleted && di.CreatedAt >= fromDate)
                    .Join(
                        _context.Ingredients.Where(i => !i.IsDeleted),
                        di => di.IngredientID,
                        i => i.IngredientID,
                        (_, i) => i.Name)
                    .ToListAsync();

                var allergenMentions = ingredientNames
                    .Where(name => knownAllergens.Contains(name.ToLower()))
                    .ToList();

                var total = allergenMentions.Count;
                response.TotalRecords = total;

                response.Data = allergenMentions
                    .GroupBy(name => name)
                    .Select(g => new ReportItemDto
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
            }


            //restaurant safety ratios report groups
            else if (request.ReportType == "RestaurantSafetyRatios")
            {
                var menus = await _context.MenuUploads
                    .Where(m => !m.IsDeleted && m.UploadDate >= fromDate)
                    .Include(m => m.Dishes)
                    .ToListAsync();

                var total = menus.Count;
                response.TotalRecords = total;

                response.Data = menus
                    .GroupBy(m =>
                    {
                        var dishes = m.Dishes.Where(d => !d.IsDeleted).ToList();
                        if (dishes.Count == 0) return "No Data";

                        var unsafeCount = dishes.Count(d => !d.IsSafe);
                        if (unsafeCount == 0) return "Safe";
                        if (unsafeCount == dishes.Count) return "Dangerous";

                        return "Warning";
                    })
                    .Select(g => new ReportItemDto
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .ToList();
            }

            //app usage analytics report groups
            else if (request.ReportType == "AppUsageAnalytics")
            {
                var scans = await _context.ScanHistories
                    .Where(s => !s.IsDeleted && s.ScanDate >= fromDate)
                    .ToListAsync();

                var total = scans.Count;
                response.TotalRecords = total;

                response.Data = scans
                    .GroupBy(s => s.UserID)
                    .Select(g => new ReportItemDto
                    {
                        Category = "User " + g.Key,
                        Count = g.Count(),
                        Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();
            }


            //scan activity trends report groups
            else if (request.ReportType == "ScanActivityTrends")
            {
                var scans = await _context.ScanHistories
                    .Where(s => !s.IsDeleted && s.ScanDate >= fromDate)
                    .ToListAsync();

                var total = scans.Count;
                response.TotalRecords = total;

                response.Data = scans
                    .GroupBy(s => s.ScanDate.Date)
                    .Select(g => new ReportItemDto
                    {
                        Category = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count(),
                        Percentage = 0
                    })
                    .OrderBy(x => x.Category)
                    .ToList();
            }
            else
            {
                return BadRequest("Invalid report type.");
            }

            return Ok(response);
        }


        //export report generates a downloadable file 
        [HttpPost("export-report")]
        public async Task<IActionResult> ExportReport(ExportReportRequestDto request)
        {
            // Generate the report data based on the requested type and date range
            var reportResult = await GenerateAnalyticsData(request.ReportType, request.DateRange);

            if (reportResult == null || !reportResult.Data.Any())
                return BadRequest("No data available for export.");

            // Generate CSV 
            if (request.Format == "CSV")
            {
                var csv = new StringBuilder();

                csv.AppendLine("Category,Count,Percentage");

                foreach (var item in reportResult.Data)
                {
                    csv.AppendLine($"{item.Category},{item.Count},{item.Percentage}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());

                return File(bytes,
                    "text/csv",
                    $"{request.ReportType}_{DateTime.UtcNow:yyyyMMdd}.csv");
            }

            // Default to Excel if not CSV
            else
            {
                using var package = new OfficeOpenXml.ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Report");

                worksheet.Cells[1, 1].Value = "Category";
                worksheet.Cells[1, 2].Value = "Count";
                worksheet.Cells[1, 3].Value = "Percentage";

                int row = 2;

                foreach (var item in reportResult.Data)
                {
                    worksheet.Cells[row, 1].Value = item.Category;
                    worksheet.Cells[row, 2].Value = item.Count;
                    worksheet.Cells[row, 3].Value = item.Percentage;
                    row++;
                }

                var fileBytes = package.GetAsByteArray();

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{request.ReportType}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
        }
        // Helper method to generate analytics data for export
        private async Task<GenerateReportResponseDto> GenerateAnalyticsData(string reportType, string dateRange)
        {
            var request = new GenerateReportRequestDto
            {
                ReportType = reportType,
                DateRange = dateRange
            };
            var result = await GenerateAnalyticsReport(request) as OkObjectResult;

            return result?.Value as GenerateReportResponseDto ?? new GenerateReportResponseDto();
        }


    }
}
