using JobRecommendationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace JobRecommendationSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Job> Jobs { get; set; }
    }
}
