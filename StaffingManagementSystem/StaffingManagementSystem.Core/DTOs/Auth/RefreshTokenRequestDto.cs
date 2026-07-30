using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>Payload for POST /api/auth/refresh.</summary>
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
