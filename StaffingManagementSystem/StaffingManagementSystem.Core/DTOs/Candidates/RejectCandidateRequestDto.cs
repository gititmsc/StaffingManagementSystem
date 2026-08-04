using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Payload for POST /api/candidates/{id}/reject.</summary>
    public class RejectCandidateRequestDto
    {
        [Required(ErrorMessage = "A rejection comment is required.")]
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}
