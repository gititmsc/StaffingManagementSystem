using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>EF Core mapping for <see cref="CandidateEducation"/> -&gt; dbo.CandidateEducation.</summary>
    public class CandidateEducationConfiguration : IEntityTypeConfiguration<CandidateEducation>
    {
        public void Configure(EntityTypeBuilder<CandidateEducation> builder)
        {
            builder.ToTable("CandidateEducation");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(e => e.CandidateId)
                .IsRequired();

            builder.Property(e => e.Degree)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.Institution)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.FieldOfStudy)
                .HasMaxLength(150);

            builder.Property(e => e.Grade)
                .HasMaxLength(50);

            builder.Property(e => e.IsExpected)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}
