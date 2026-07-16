namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>
    /// A single-use, time-limited token issued for the "forgot password" flow.
    /// Only the SHA-256 hash of the raw token is persisted; the raw token is emailed
    /// to the user and never stored.
    /// </summary>
    public class PasswordResetToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        /// <summary>Base64-encoded SHA-256 hash of the raw reset token.</summary>
        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        /// <summary>Set once the token has been redeemed; a used token can never be redeemed again.</summary>
        public DateTime? UsedAtUtc { get; set; }
    }
}
