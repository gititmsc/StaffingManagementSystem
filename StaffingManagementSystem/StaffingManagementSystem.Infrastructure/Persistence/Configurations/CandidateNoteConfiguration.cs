using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>EF Core mapping for <see cref="CandidateNote"/> -&gt; dbo.CandidateNotes.</summary>
    public class CandidateNoteConfiguration : IEntityTypeConfiguration<CandidateNote>
    {
        public void Configure(EntityTypeBuilder<CandidateNote> builder)
        {
            builder.ToTable("CandidateNotes");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(n => n.CandidateId)
                .IsRequired();

            builder.Property(n => n.Note)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(n => n.CreatedByUserId)
                .IsRequired();

            builder.Property(n => n.CreatedAtUtc)
                .IsRequired();
        }
    }
}
