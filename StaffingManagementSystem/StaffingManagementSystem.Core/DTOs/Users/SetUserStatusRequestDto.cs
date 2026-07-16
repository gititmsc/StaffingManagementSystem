using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Users
{
    /// <summary>
    /// Payload for PATCH /api/users/{id}/status.
    /// </summary>
    public class SetUserStatusRequestDto
    {
        [Required(ErrorMessage = "IsActive is required.")]
        public bool IsActive { get; set; }
    }
}
