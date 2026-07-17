using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>EF Core mapping for <see cref="CandidateAttachment"/> -&gt; dbo.CandidateAttachments.</summary>
    public class CandidateAttachmentConfiguration : IEntityTypeConfiguration<CandidateAttachment>
    {
        public void Configure(EntityTypeBuilder<CandidateAttachment> builder)
        {
            builder.ToTable("CandidateAttachments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(a => a.CandidateId)
                .IsRequired();

            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(260);

            builder.Property(a => a.StoredPath)
                .IsRequired()
                .HasMaxLength(400);

            builder.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.FileSizeBytes)
                .IsRequired();

            builder.Property(a => a.UploadedByUserId)
                .IsRequired();

            builder.Property(a => a.UploadedAtUtc)
                .IsRequired();
        }
    }
}
