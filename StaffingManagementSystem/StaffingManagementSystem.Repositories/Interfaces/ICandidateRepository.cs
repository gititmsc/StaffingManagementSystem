using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Enums;

namespace StaffingManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for the <see cref="Candidate"/> aggregate (candidate profile plus
    /// its skills, experience, education, project and note child records).
    /// </summary>
    public interface ICandidateRepository
    {
        /// <summary>Every non-deleted candidate, with skills and experience included for list-view display.</summary>
        Task<List<Candidate>> GetAllAsync();

        /// <summary>A single non-deleted candidate with its full graph (skills, experience, education, projects, notes).</summary>
        Task<Candidate?> GetByIdAsync(Guid id);

        /// <summary>Lightweight existence check — does not load the candidate's child graph.</summary>
        Task<bool> ExistsAsync(Guid id);

        Task<bool> EmailExistsAsync(string email, Guid? excludeCandidateId = null);

        /// <summary>
        /// True if a non-deleted, non-Rejected candidate already exists with this email or
        /// phone — used by the public self-registration form to block duplicate submissions.
        /// A prior Rejected record does not count, so a candidate can re-apply after rejection.
        /// </summary>
        Task<bool> HasActiveDuplicateAsync(string email, string? phone);

        /// <summary>Finds a skill by case-insensitive name, creating it in the shared skill master list if needed.</summary>
        Task<Guid> GetOrCreateSkillIdAsync(string skillName);

        Task CreateAsync(Candidate candidate);

        /// <summary>
        /// Replaces a candidate's scalar fields and its entire skills/experience/education/projects
        /// graph in one transaction. Notes are append-only and not touched here.
        /// </summary>
        Task UpdateAsync(
            Candidate candidate,
            List<CandidateSkill> skills,
            List<CandidateExperience> experience,
            List<CandidateEducation> education,
            List<CandidateProject> projects);

        /// <summary>
        /// Lean update of just the approval-workflow fields (status, rejection comment,
        /// approve/reject audit) — does not touch the skills/experience/education/projects
        /// graph, unlike <see cref="UpdateAsync"/>.
        /// </summary>
        Task UpdateApprovalStatusAsync(
            Guid candidateId,
            CandidateStatus status,
            string? rejectionComment,
            Guid? approvedByUserId,
            DateTime? approvedAtUtc,
            Guid? rejectedByUserId,
            DateTime? rejectedAtUtc);

        /// <summary>Soft-deletes a candidate: hidden from all views but retained for history.</summary>
        Task SoftDeleteAsync(Guid id);

        Task AddNoteAsync(CandidateNote note);
    }
}
