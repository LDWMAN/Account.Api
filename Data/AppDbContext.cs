using AccountApi.Model.Entity;
using Microsoft.EntityFrameworkCore;

namespace AccountApi.Data
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<RefreshToken> RefreshToken { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}