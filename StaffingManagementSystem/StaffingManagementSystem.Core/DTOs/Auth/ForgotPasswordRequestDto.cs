using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>
    /// Payload for POST /api/auth/forgot-password.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}
