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

        public DbSet<Candidate> Candidates => Set<Candidate>();

        public DbSet<SkillMaster> SkillMaster => Set<SkillMaster>();

        public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();

        public DbSet<CandidateExperience> CandidateExperience => Set<CandidateExperience>();

        public DbSet<CandidateEducation> CandidateEducation => Set<CandidateEducation>();

        public DbSet<CandidateProject> CandidateProjects => Set<CandidateProject>();

        public DbSet<CandidateNote> CandidateNotes => Set<CandidateNote>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
