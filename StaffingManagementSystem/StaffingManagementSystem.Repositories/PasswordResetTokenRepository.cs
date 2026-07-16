using Microsoft.EntityFrameworkCore;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Infrastructure.Persistence;
using StaffingManagementSystem.Repositories.Interfaces;

namespace StaffingManagementSystem.Repositories
{
    /// <inheritdoc cref="IPasswordResetTokenRepository"/>
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _dbContext;

        public PasswordResetTokenRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(PasswordResetToken token)
        {
            await _dbContext.PasswordResetTokens.AddAsync(token);
            await _dbContext.SaveChangesAsync();
        }

        public Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, DateTime nowUtc)
            => _dbContext.PasswordResetTokens
                .Where(t => t.TokenHash == tokenHash && t.UsedAtUtc == null && t.ExpiresAtUtc > nowUtc)
                .FirstOrDefaultAsync();

        public async Task MarkUsedAsync(Guid tokenId, DateTime usedAtUtc)
        {
            await _dbContext.PasswordResetTokens
                .Where(t => t.Id == tokenId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.UsedAtUtc, usedAtUtc));
        }

        public async Task InvalidateActiveTokensForUserAsync(Guid userId)
        {
            await _dbContext.PasswordResetTokens
                .Where(t => t.UserId == userId && t.UsedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.UsedAtUtc, DateTime.UtcNow));
        }
    }
}
