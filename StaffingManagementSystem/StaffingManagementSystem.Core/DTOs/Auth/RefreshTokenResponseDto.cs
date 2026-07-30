namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>Result returned by a successful POST /api/auth/refresh.</summary>
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>The rotated replacement refresh token — the client must store this and discard the old one.</summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
