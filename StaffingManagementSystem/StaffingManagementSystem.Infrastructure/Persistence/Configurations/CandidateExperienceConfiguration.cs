using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>EF Core mapping for <see cref="CandidateExperience"/> -&gt; dbo.CandidateExperience.</summary>
    public class CandidateExperienceConfiguration : IEntityTypeConfiguration<CandidateExperience>
    {
        public void Configure(EntityTypeBuilder<CandidateExperience> builder)
        {
            builder.ToTable("CandidateExperience");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(e => e.CandidateId)
                .IsRequired();

            builder.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.JobTitle)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.EmploymentType)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(e => e.StartDate)
                .IsRequired();

            builder.Property(e => e.Location)
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .HasMaxLength(2000);
        }
    }
}
