using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Repositories.Interfaces
{
    /// <summary>Data access contract for <see cref="CandidateAttachment"/> records.</summary>
    public interface ICandidateAttachmentRepository
    {
        Task<List<CandidateAttachment>> GetByCandidateIdAsync(Guid candidateId);

        Task<CandidateAttachment?> GetByIdAsync(Guid attachmentId);

        Task AddAsync(CandidateAttachment attachment);

        Task DeleteAsync(CandidateAttachment attachment);
    }
}
