using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for <see cref="RefreshToken"/> records.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken token);

        /// <summary>
        /// Returns the token matching <paramref name="tokenHash"/> if it exists, has not been
        /// revoked, and has not expired as of <paramref name="nowUtc"/>; otherwise null.
        /// </summary>
        Task<RefreshToken?> GetValidByTokenHashAsync(string tokenHash, DateTime nowUtc);

        /// <summary>Revokes a single token, optionally recording the token that replaced it (rotation).</summary>
        Task RevokeAsync(Guid tokenId, DateTime revokedAtUtc, Guid? replacedByTokenId = null);

        /// <summary>
        /// Revokes every currently-active token for the user — used on logout-everywhere and
        /// after a password reset, so old sessions can no longer silently refresh.
        /// </summary>
        Task RevokeAllForUserAsync(Guid userId, DateTime revokedAtUtc);
    }
}
