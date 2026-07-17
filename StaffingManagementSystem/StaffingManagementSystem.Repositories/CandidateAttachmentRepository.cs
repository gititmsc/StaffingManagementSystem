using Microsoft.EntityFrameworkCore;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Infrastructure.Persistence;
using StaffingManagementSystem.Repositories.Interfaces;

namespace StaffingManagementSystem.Repositories
{
    /// <inheritdoc cref="ICandidateAttachmentRepository"/>
    public class CandidateAttachmentRepository : ICandidateAttachmentRepository
    {
        private readonly AppDbContext _dbContext;

        public CandidateAttachmentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<CandidateAttachment>> GetByCandidateIdAsync(Guid candidateId)
            => _dbContext.CandidateAttachments
                .Where(a => a.CandidateId == candidateId)
                .OrderByDescending(a => a.UploadedAtUtc)
                .ToListAsync();

        public Task<CandidateAttachment?> GetByIdAsync(Guid attachmentId)
            => _dbContext.CandidateAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId);

        public async Task AddAsync(CandidateAttachment attachment)
        {
            await _dbContext.CandidateAttachments.AddAsync(attachment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(CandidateAttachment attachment)
        {
            _dbContext.CandidateAttachments.Remove(attachment);
            await _dbContext.SaveChangesAsync();
        }
    }
}
