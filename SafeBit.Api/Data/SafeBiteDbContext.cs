using Microsoft.EntityFrameworkCore;
using SafeBit.Api.Model;

namespace SafeBit.Api.Data
{
    public class SafeBiteDbContext : DbContext
    {
        public SafeBiteDbContext(DbContextOptions<SafeBiteDbContext> options)
            : base(options)
        {
        }



        // Tables Regestrations 
        public DbSet<Role> Roles { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<Disease> Diseases { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserAllergy> UserAllergies { get; set; }
        public DbSet<UserDisease> UserDiseases { get; set; }

        public DbSet<MenuUpload> MenuUploads => Set<MenuUpload>();
        public DbSet<Dish> Dishes => Set<Dish>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<DishIngredient> DishIngredients => Set<DishIngredient>();
        public DbSet<ScanHistory> ScanHistories => Set<ScanHistory>();
        public DbSet<FeedbackReport> FeedbackReports => Set<FeedbackReport>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("safebit");

            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<UserAllergy>()
     .HasKey(ua => new { ua.UserID, ua.AllergyID });

            modelBuilder.Entity<UserDisease>()
                .HasKey(ud => new { ud.UserID, ud.DiseaseID });

            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, Type = "Admin" },
                new Role { RoleID = 2, Type = "User" }
            );

            // Allergies
            modelBuilder.Entity<Allergy>().HasData(
                new Allergy { AllergyID = 1, Name = "Peanuts", Category = "Food", CreatedAt = DateTime.UtcNow },
                new Allergy { AllergyID = 2, Name = "Milk", Category = "Dairy", CreatedAt = DateTime.UtcNow },
                new Allergy { AllergyID = 3, Name = "Eggs", Category = "Food", CreatedAt = DateTime.UtcNow },
                new Allergy { AllergyID = 4, Name = "Seafood", Category = "Food", CreatedAt = DateTime.UtcNow }
            );

            // Diseases
            modelBuilder.Entity<Disease>().HasData(
                new Disease { DiseaseID = 1, Name = "Diabetes", Category = "Chronic", CreatedAt = DateTime.UtcNow },
                new Disease { DiseaseID = 2, Name = "Celiac Disease", Category = "Digestive", CreatedAt = DateTime.UtcNow },
                new Disease { DiseaseID = 3, Name = "Hypertension", Category = "Chronic", CreatedAt = DateTime.UtcNow }
            );

            // MenuUpload
            modelBuilder.Entity<MenuUpload>()
                .HasKey(m => m.MenuID);

            modelBuilder.Entity<MenuUpload>()
                .Property(m => m.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<MenuUpload>()
                .HasMany(m => m.Dishes)
                .WithOne(d => d.MenuUpload)
                .HasForeignKey(d => d.MenuID)
                .OnDelete(DeleteBehavior.Cascade);

            // Dish
            modelBuilder.Entity<Dish>()
                .HasKey(d => d.DishID);

            modelBuilder.Entity<Dish>()
                .Property(d => d.DishName)
                .IsRequired()
                .HasMaxLength(200);

            // Ingredient
            modelBuilder.Entity<Ingredient>()
                .HasKey(i => i.IngredientID);

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(150);

            // DishIngredient (composite PK)
            modelBuilder.Entity<DishIngredient>()
                .HasKey(di => new { di.DishID, di.IngredientID });

            modelBuilder.Entity<DishIngredient>()
                .HasOne(di => di.Dish)
                .WithMany(d => d.DishIngredients)
                .HasForeignKey(di => di.DishID);

            modelBuilder.Entity<DishIngredient>()
                .HasOne(di => di.Ingredient)
                .WithMany(i => i.DishIngredients)
                .HasForeignKey(di => di.IngredientID);

            // ScanHistory
            modelBuilder.Entity<ScanHistory>()
                .HasKey(s => s.ScanID);

            modelBuilder.Entity<ScanHistory>()
                .HasOne(s => s.MenuUpload)
                .WithMany(m => m.ScanHistories)
                .HasForeignKey(s => s.MenuID)
                .OnDelete(DeleteBehavior.Restrict);

            // FeedbackReport
            modelBuilder.Entity<FeedbackReport>()
                .HasKey(f => f.ReportID);

            modelBuilder.Entity<FeedbackReport>()
                .Property(f => f.Message)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<FeedbackReport>()
                .Property(f => f.Status)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<FeedbackReport>()
                .HasOne(f => f.Dish)
                .WithMany(d => d.FeedbackReports)
                .HasForeignKey(f => f.DishID)
                .OnDelete(DeleteBehavior.Restrict);




        }





    }
}
