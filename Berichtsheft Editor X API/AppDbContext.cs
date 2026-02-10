using Berichtsheft_Editor_X.Models;
using Microsoft.EntityFrameworkCore;

namespace Berichtsheft_Editor_X_API
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Bericht> Bericht { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define One-to-Many Relationship
            modelBuilder.Entity<Bericht>()
                .HasOne(b => b.User)
                .WithMany(u => u.Berichte)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
