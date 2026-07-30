namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>
    /// Result returned to the client after a successful login.
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Long-lived opaque token the client stores and later exchanges via
        /// POST /api/auth/refresh for a new access token, without the user signing in again.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        public UserDto User { get; set; } = new();
    }
}
