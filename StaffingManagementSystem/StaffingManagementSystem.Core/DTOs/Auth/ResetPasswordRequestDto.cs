using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>
    /// Payload for POST /api/auth/reset-password.
    /// </summary>
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Reset token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
