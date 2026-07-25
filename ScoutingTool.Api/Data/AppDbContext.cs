using Microsoft.EntityFrameworkCore;
using ScoutingTool.Api.Models;

namespace ScoutingTool.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // These DbSets represent our tables in the database
        public DbSet<Player> Players { get; set; }
        public DbSet<ScoutingSource> ScoutingSources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .HasIndex(p => new { p.Name, p.ClubTeamName })
                .IsUnique();

            // Here, we can define extra rules (like making sure a name is required)
            modelBuilder.Entity<ScoutingSource>()
                .Property(p => p.Name)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .Property(p => p.Name)
                .IsRequired();
        }
    }
}