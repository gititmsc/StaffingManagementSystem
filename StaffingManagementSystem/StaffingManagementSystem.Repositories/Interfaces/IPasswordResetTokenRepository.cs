using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for <see cref="PasswordResetToken"/> records.
    /// </summary>
    public interface IPasswordResetTokenRepository
    {
        Task CreateAsync(PasswordResetToken token);

        /// <summary>
        /// Returns the token matching <paramref name="tokenHash"/> if it exists, has not
        /// been used, and has not expired as of <paramref name="nowUtc"/>; otherwise null.
        /// </summary>
        Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, DateTime nowUtc);

        Task MarkUsedAsync(Guid tokenId, DateTime usedAtUtc);

        /// <summary>Marks every currently-unused token for the user as used, so it can no longer be redeemed.</summary>
        Task InvalidateActiveTokensForUserAsync(Guid userId);
    }
}
