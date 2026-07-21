using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Users
{
    /// <summary>
    /// Payload for PUT /api/users/{id}. Email is intentionally not editable here — it is the
    /// account's login identifier and changing it is out of scope for this release.
    /// </summary>
    public class UpdateUserRequestDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        public string? Department { get; set; }

        /// <summary>One of: Admin, Recruiter, Viewer.</summary>
        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;
    }
}
