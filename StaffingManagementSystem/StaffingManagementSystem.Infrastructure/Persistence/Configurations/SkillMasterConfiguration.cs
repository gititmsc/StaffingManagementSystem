using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>EF Core mapping for <see cref="SkillMaster"/> -&gt; dbo.SkillMaster.</summary>
    public class SkillMasterConfiguration : IEntityTypeConfiguration<SkillMaster>
    {
        public void Configure(EntityTypeBuilder<SkillMaster> builder)
        {
            builder.ToTable("SkillMaster");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(s => s.Name)
                .IsUnique();

            builder.Property(s => s.Category)
                .HasMaxLength(100);
        }
    }
}
