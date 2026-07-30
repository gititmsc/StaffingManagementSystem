namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>
    /// A long-lived, single-use-then-rotated token that lets the client silently obtain a new
    /// JWT access token without asking the user to sign in again. Only the SHA-256 hash of the
    /// raw token is persisted; the raw token is returned to the client once and never stored.
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        /// <summary>Base64-encoded SHA-256 hash of the raw refresh token.</summary>
        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Set once this token has been used (rotated) or explicitly revoked (logout, password
        /// reset). A revoked token can never be redeemed again.
        /// </summary>
        public DateTime? RevokedAtUtc { get; set; }

        /// <summary>The token that replaced this one when it was rotated, if any.</summary>
        public Guid? ReplacedByTokenId { get; set; }
    }
}
