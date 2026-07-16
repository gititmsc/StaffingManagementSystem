using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Business logic contract for the candidate master module (RMS SRS Section 3.3).
    /// </summary>
    public interface ICandidateService
    {
        Task<ApiResponse<List<CandidateListItemDto>>> GetAllCandidatesAsync();

        Task<ApiResponse<CandidateDetailDto>> GetCandidateByIdAsync(Guid id);

        /// <summary>Creates a candidate, owned by <paramref name="ownerRecruiterId"/> (the creating user).</summary>
        Task<ApiResponse<CandidateDetailDto>> CreateCandidateAsync(CreateCandidateRequestDto request, Guid ownerRecruiterId);

        /// <summary>Replaces a candidate's profile fields and entire skills/experience/education/projects graph.</summary>
        Task<ApiResponse<CandidateDetailDto>> UpdateCandidateAsync(Guid id, UpdateCandidateRequestDto request);

        Task<ApiResponse<object>> DeleteCandidateAsync(Guid id);

        Task<ApiResponse<CandidateNoteDto>> AddNoteAsync(Guid candidateId, AddCandidateNoteRequestDto request, Guid createdByUserId);
    }
}
