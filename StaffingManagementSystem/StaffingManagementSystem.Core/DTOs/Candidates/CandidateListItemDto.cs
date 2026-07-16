namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Row shape returned by GET /api/candidates.</summary>
    public class CandidateListItemDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? CurrentLocation { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal TotalExperienceYears { get; set; }

        /// <summary>Company name from the experience entry flagged IsCurrent, if any.</summary>
        public string? CurrentCompany { get; set; }

        public List<string> Skills { get; set; } = new();

        public Guid OwnerRecruiterId { get; set; }

        public string? OwnerRecruiterName { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
