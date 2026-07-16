using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="PasswordResetToken"/> -&gt; dbo.PasswordResetTokens.
    /// </summary>
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(t => t.TokenHash);

            builder.HasIndex(t => t.UserId);

            builder.Property(t => t.ExpiresAtUtc)
                .IsRequired();

            builder.Property(t => t.CreatedAtUtc)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
