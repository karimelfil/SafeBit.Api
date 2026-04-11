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

        // Tables Registrations
        public DbSet<Role> Roles { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<Disease> Diseases { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAllergy> UserAllergies { get; set; }
        public DbSet<UserDisease> UserDiseases { get; set; }
        public DbSet<MenuUpload> MenuUploads { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<DishIngredient> DishIngredients { get; set; }
        public DbSet<ScanHistory> ScanHistories { get; set; }
        public DbSet<FeedbackReport> FeedbackReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("safebit");

            // Composite Keys
            modelBuilder.Entity<UserAllergy>()
                .HasKey(ua => new { ua.UserID, ua.AllergyID });

            modelBuilder.Entity<UserDisease>()
                .HasKey(ud => new { ud.UserID, ud.DiseaseID });

            modelBuilder.Entity<DishIngredient>()
                .HasKey(di => new { di.DishID, di.IngredientID });

            // Roles Seed
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, Type = "Admin" },
                new Role { RoleID = 2, Type = "User" }
            );

            // MenuUpload
            modelBuilder.Entity<MenuUpload>(entity =>
            {
                entity.ToTable("MenuUploads", "safebit");

                entity.HasKey(m => m.MenuID);

                entity.Property(m => m.FilePath)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasMany(m => m.Dishes)
                    .WithOne(d => d.MenuUpload)
                    .HasForeignKey(d => d.MenuID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Dish
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.ToTable("Dishes", "safebit");

                entity.HasKey(d => d.DishID);

                entity.Property(d => d.DishName)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            // Ingredient
            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.ToTable("Ingredients", "safebit");

                entity.HasKey(i => i.IngredientID);

                entity.Property(i => i.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            // DishIngredient
            modelBuilder.Entity<DishIngredient>(entity =>
            {
                entity.ToTable("DishIngredients", "safebit");

                entity.HasKey(di => new { di.DishID, di.IngredientID });

                entity.HasOne(di => di.Dish)
                    .WithMany(d => d.DishIngredients)
                    .HasForeignKey(di => di.DishID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(di => di.Ingredient)
                    .WithMany(i => i.DishIngredients)
                    .HasForeignKey(di => di.IngredientID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ScanHistory
            modelBuilder.Entity<ScanHistory>(entity =>
            {
                entity.ToTable("ScanHistories", "safebit");

                entity.HasKey(s => s.ScanID);

                entity.HasOne(s => s.MenuUpload)
                    .WithMany(m => m.ScanHistories)
                    .HasForeignKey(s => s.MenuID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // FeedbackReport
            modelBuilder.Entity<FeedbackReport>(entity =>
            {
                entity.ToTable("FeedbackReports", "safebit");

                entity.HasKey(f => f.ReportID);

                entity.Property(f => f.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(f => f.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(f => f.Dish)
                    .WithMany(d => d.FeedbackReports)
                    .HasForeignKey(f => f.DishID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
