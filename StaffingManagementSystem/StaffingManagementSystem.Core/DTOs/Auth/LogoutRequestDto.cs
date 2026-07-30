namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>
    /// Payload for POST /api/auth/logout. RefreshToken is optional — if provided, it is
    /// revoked server-side so it can never be used to silently obtain a new access token again.
    /// </summary>
    public class LogoutRequestDto
    {
        public string? RefreshToken { get; set; }
    }
}
