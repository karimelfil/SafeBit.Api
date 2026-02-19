    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using SafeBit.Api.Data;
    using SafeBit.Api.DTOs.Menu;
    using SafeBit.Api.Model;
using System.Text.Json;

    public class MenuAnalysisService
    {
        private readonly SafeBiteDbContext _db;
        private readonly IWebHostEnvironment _env;

        public MenuAnalysisService(SafeBiteDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // Saves the uploaded menu and AI analysis results to the database, returning the MenuID.
        public async Task<int> CreateMenuAndSaveResultAsync(
            int userId,
            string restaurantName,
            IFormFile file,
            AiAnalyzeMenuResponse aiResult)
        {

            var rootPath = _env.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadsDir = Path.Combine(rootPath, "uploads", "menus");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/menus/{fileName}";


            var menu = new MenuUpload
            {
                UserID = userId,
                RestaurantName = restaurantName.Trim(),
                FilePath = relativePath,
                UploadDate = DateTime.UtcNow
            };

            _db.MenuUploads.Add(menu);
            await _db.SaveChangesAsync();


            foreach (var d in aiResult.Dishes)
            {
                if (string.IsNullOrWhiteSpace(d.DishName))
                    continue;

                var dish = new Dish
                {
                    MenuID = menu.MenuID,
                    DishName = d.DishName.Trim(),
                    IsSafe = d.SafetyLevel == "SAFE"
                };

                _db.Dishes.Add(dish);
                await _db.SaveChangesAsync();


                foreach (var ingRaw in d.IngredientsFound.Distinct())
                {
                    var normalized = ingRaw.Trim().ToLower();

                    var ingredient = await _db.Ingredients
                        .FirstOrDefaultAsync(i =>
                            i.Name.ToLower() == normalized &&
                            !i.IsDeleted);

                    if (ingredient == null)
                    {
                        ingredient = new Ingredient
                        {
                            Name = normalized,
                            CreatedAt = DateTime.UtcNow
                        };

                        _db.Ingredients.Add(ingredient);
                        await _db.SaveChangesAsync();
                    }

                    var exists = await _db.DishIngredients.AnyAsync(di =>
                        di.DishID == dish.DishID &&
                        di.IngredientID == ingredient.IngredientID &&
                        !di.IsDeleted);

                    if (!exists)
                    {
                        _db.DishIngredients.Add(new DishIngredient
                        {
                            DishID = dish.DishID,
                            IngredientID = ingredient.IngredientID,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

        var dishesJson = JsonSerializer.Serialize(aiResult.Dishes);

        _db.ScanHistories.Add(new ScanHistory
        {
            UserID = userId,
            MenuID = menu.MenuID,
            ScanDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,

            ResultsSummary = dishesJson, 

            CreatedAt = DateTime.UtcNow
        });


        await _db.SaveChangesAsync();

            return menu.MenuID;
        }


    }
