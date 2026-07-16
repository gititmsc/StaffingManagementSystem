using Microsoft.EntityFrameworkCore;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core database context for the Staffing Management System.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
