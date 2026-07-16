using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>EF Core mapping for <see cref="CandidateSkill"/> -&gt; dbo.CandidateSkills.</summary>
    public class CandidateSkillConfiguration : IEntityTypeConfiguration<CandidateSkill>
    {
        public void Configure(EntityTypeBuilder<CandidateSkill> builder)
        {
            builder.ToTable("CandidateSkills");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(s => s.CandidateId)
                .IsRequired();

            builder.Property(s => s.SkillId)
                .IsRequired();

            builder.HasIndex(s => s.SkillId);

            builder.HasOne(s => s.Skill)
                .WithMany()
                .HasForeignKey(s => s.SkillId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.Proficiency)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(s => s.YearsOfExperience)
                .HasColumnType("decimal(4,1)");
        }
    }
}
