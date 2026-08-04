using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>
    /// Payload for POST /api/auth/change-password — a signed-in user changing their own
    /// password (as opposed to the token-based forgot/reset-password flow for a signed-out user).
    /// </summary>
    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
