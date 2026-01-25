using Microsoft.EntityFrameworkCore;

namespace SafeBit.Api.Data
{
    public class SafeBiteDbContext : DbContext
    {
        public SafeBiteDbContext(DbContextOptions<SafeBiteDbContext> options)
            : base(options)
        {
        }
    }
}
