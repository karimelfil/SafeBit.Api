using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Data;
using SafeBit.Api.Model;
using System.Globalization;

namespace SafeBit.Api.Services
{
    public class MasterDataSeedService
    {
        private readonly SafeBiteDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public MasterDataSeedService(SafeBiteDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }
        
        
        public async Task SeedAsync()
        {
            var seedRoot = Path.Combine(_environment.ContentRootPath, "Data", "SeedData");

            await SeedAllergiesAsync(Path.Combine(seedRoot, "allergies.csv"));
            await SeedDiseasesAsync(Path.Combine(seedRoot, "diseases.csv"));
        }

        
        private async Task SeedAllergiesAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var rows = await ReadCsvAsync(filePath);
            if (rows.Count == 0)
                return;

            var existingById = await _db.Allergies
                .IgnoreQueryFilters()
                .ToDictionaryAsync(x => x.AllergyID);

            foreach (var row in rows)
            {
                if (!TryGetRequiredValue(row, "AllergyID", out var idValue) ||
                    !int.TryParse(idValue, out var allergyId) ||
                    !TryGetRequiredValue(row, "Name", out var name) ||
                    !TryGetRequiredValue(row, "Category", out var category) ||
                    !TryGetRequiredValue(row, "CreatedAt", out var createdAtValue) ||
                    !DateTime.TryParse(
                        createdAtValue,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out var createdAt))
                {
                    continue;
                }

                if (existingById.TryGetValue(allergyId, out var existing))
                {
                    existing.Name = name;
                    existing.Category = category;
                    existing.CreatedAt = createdAt;
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = "CSV Seeder";
                }
                else
                {
                    var allergy = new Allergy
                    {
                        AllergyID = allergyId,
                        Name = name,
                        Category = category,
                        CreatedAt = createdAt,
                        IsDeleted = false
                    };

                    _db.Allergies.Add(allergy);
                    existingById.Add(allergyId, allergy);
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task SeedDiseasesAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var rows = await ReadCsvAsync(filePath);
            if (rows.Count == 0)
                return;

            var existingById = await _db.Diseases
                .IgnoreQueryFilters()
                .ToDictionaryAsync(x => x.DiseaseID);

            foreach (var row in rows)
            {
                if (!TryGetRequiredValue(row, "DiseaseID", out var idValue) ||
                    !int.TryParse(idValue, out var diseaseId) ||
                    !TryGetRequiredValue(row, "Name", out var name) ||
                    !TryGetRequiredValue(row, "Category", out var category) ||
                    !TryGetRequiredValue(row, "CreatedAt", out var createdAtValue) ||
                    !DateTime.TryParse(
                        createdAtValue,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out var createdAt))
                {
                    continue;
                }

                if (existingById.TryGetValue(diseaseId, out var existing))
                {
                    existing.Name = name;
                    existing.Category = category;
                    existing.CreatedAt = createdAt;
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = "CSV Seeder";
                }
                else
                {
                    var disease = new Disease
                    {
                        DiseaseID = diseaseId,
                        Name = name,
                        Category = category,
                        CreatedAt = createdAt,
                        IsDeleted = false
                    };

                    _db.Diseases.Add(disease);
                    existingById.Add(diseaseId, disease);
                }
            }

            await _db.SaveChangesAsync();
        }

        private static bool TryGetRequiredValue(
            IReadOnlyDictionary<string, string> row,
            string column,
            out string value)
        {
            if (row.TryGetValue(column, out value!) &&
                !string.IsNullOrWhiteSpace(value))
            {
                value = value.Trim();
                return true;
            }

            value = string.Empty;
            return false;
        }

        private static async Task<List<Dictionary<string, string>>> ReadCsvAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var rows = new List<Dictionary<string, string>>();

            if (lines.Length <= 1)
                return rows;

            var headers = SplitCsvLine(lines[0]);

            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var values = SplitCsvLine(lines[i]);
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (var j = 0; j < headers.Count; j++)
                {
                    var header = headers[j].Trim();
                    var value = j < values.Count ? values[j].Trim() : string.Empty;
                    row[header] = value;
                }

                rows.Add(row);
            }

            return rows;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var values = new List<string>();
            var current = new System.Text.StringBuilder();
            var inQuotes = false;

            foreach (var ch in line)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (ch == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            values.Add(current.ToString());
            return values;
        }
    }
}
