using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Business logic contract for the candidate master module (RMS SRS Section 3.3).
    /// </summary>
    public interface ICandidateService
    {
        /// <summary>
        /// Field-level visibility is driven by <paramref name="actorRole"/>: Viewer gets Name,
        /// Email, LinkedIn URL and Phone replaced with "XXXX"; CostToCompany is null unless
        /// the role is Admin; CurrentSalary is null unless the role is Admin or Recruiter.
        /// </summary>
        Task<ApiResponse<List<CandidateListItemDto>>> GetAllCandidatesAsync(string actorRole);

        /// <summary>
        /// Field-level visibility is driven by <paramref name="actorRole"/>: Viewer gets Name,
        /// Email, LinkedIn URL and Phone replaced with "XXXX"; CostToCompany is null unless
        /// the role is Admin; CurrentSalary is null unless the role is Admin or Recruiter.
        /// </summary>
        Task<ApiResponse<CandidateDetailDto>> GetCandidateByIdAsync(Guid id, string actorRole);

        /// <summary>
        /// Creates a candidate, owned by <paramref name="ownerRecruiterId"/> (the creating user).
        /// CostToCompany in <paramref name="request"/> is ignored unless <paramref name="actorRole"/> is Admin.
        /// </summary>
        Task<ApiResponse<CandidateDetailDto>> CreateCandidateAsync(CreateCandidateRequestDto request, Guid ownerRecruiterId, string actorRole);

        /// <summary>
        /// Replaces a candidate's profile fields and entire skills/experience/education/projects graph.
        /// CostToCompany in <paramref name="request"/> is ignored unless <paramref name="actorRole"/> is Admin.
        /// </summary>
        Task<ApiResponse<CandidateDetailDto>> UpdateCandidateAsync(Guid id, UpdateCandidateRequestDto request, string actorRole);

        Task<ApiResponse<object>> DeleteCandidateAsync(Guid id);

        Task<ApiResponse<CandidateNoteDto>> AddNoteAsync(Guid candidateId, AddCandidateNoteRequestDto request, Guid createdByUserId);
    }
}
