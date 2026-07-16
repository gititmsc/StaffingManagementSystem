using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Payload for POST /api/candidates/{id}/notes.</summary>
    public class AddCandidateNoteRequestDto
    {
        [Required(ErrorMessage = "Note text is required.")]
        [MaxLength(2000)]
        public string Note { get; set; } = string.Empty;
    }
}
