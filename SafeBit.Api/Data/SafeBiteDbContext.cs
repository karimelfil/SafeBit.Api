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
        }



    }
}
