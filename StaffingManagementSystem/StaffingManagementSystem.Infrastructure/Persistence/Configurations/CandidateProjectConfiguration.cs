using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>EF Core mapping for <see cref="CandidateProject"/> -&gt; dbo.CandidateProjects.</summary>
    public class CandidateProjectConfiguration : IEntityTypeConfiguration<CandidateProject>
    {
        public void Configure(EntityTypeBuilder<CandidateProject> builder)
        {
            builder.ToTable("CandidateProjects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(p => p.CandidateId)
                .IsRequired();

            builder.Property(p => p.ExperienceId);

            builder.Property(p => p.ProjectName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Role)
                .HasMaxLength(150);

            builder.Property(p => p.DurationText)
                .HasMaxLength(100);

            builder.Property(p => p.TechnologiesUsed)
                .HasMaxLength(500);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);
        }
    }
}
