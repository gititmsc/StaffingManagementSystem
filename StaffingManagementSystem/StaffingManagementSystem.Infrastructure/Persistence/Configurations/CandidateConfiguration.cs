using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="Candidate"/> -&gt; dbo.Candidates, including its
    /// one-to-many child collections (skills, experience, education, projects, notes).
    /// </summary>
    public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
    {
        public void Configure(EntityTypeBuilder<Candidate> builder)
        {
            builder.ToTable("Candidates");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Title)
                .HasMaxLength(200);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(256);

            // Not unique: duplicate emails are flagged as a warning (RMS SRS 3.3.1), not blocked.
            builder.HasIndex(c => c.Email);

            builder.Property(c => c.Phone)
                .HasMaxLength(30);

            builder.HasIndex(c => c.Phone);

            builder.Property(c => c.Address)
                .HasMaxLength(500);

            builder.Property(c => c.CurrentLocation)
                .HasMaxLength(200);

            builder.Property(c => c.Gender)
                .HasMaxLength(30);

            builder.Property(c => c.LinkedInUrl)
                .HasMaxLength(300);

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(c => c.Source)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(c => c.OtherSourceText)
                .HasMaxLength(200);

            builder.Property(c => c.OwnerRecruiterId)
                .IsRequired();

            builder.Property(c => c.CostToCompany)
                .HasColumnType("decimal(12,2)");

            builder.Property(c => c.CostToVendor)
                .HasColumnType("decimal(12,2)");

            builder.Property(c => c.CurrentSalary)
                .HasColumnType("decimal(12,2)");

            builder.Property(c => c.TotalExperienceYears)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0m);

            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.CreatedAtUtc)
                .IsRequired();

            builder.HasMany(c => c.Skills)
                .WithOne()
                .HasForeignKey(s => s.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Experience)
                .WithOne()
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Education)
                .WithOne()
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Projects)
                .WithOne()
                .HasForeignKey(p => p.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Notes)
                .WithOne()
                .HasForeignKey(n => n.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Attachments)
                .WithOne()
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
