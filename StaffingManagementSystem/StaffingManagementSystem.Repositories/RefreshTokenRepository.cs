using Microsoft.EntityFrameworkCore;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Infrastructure.Persistence;
using StaffingManagementSystem.Repositories.Interfaces;

namespace StaffingManagementSystem.Repositories
{
    /// <inheritdoc cref="IRefreshTokenRepository"/>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _dbContext;

        public RefreshTokenRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(RefreshToken token)
        {
            await _dbContext.RefreshTokens.AddAsync(token);
            await _dbContext.SaveChangesAsync();
        }

        public Task<RefreshToken?> GetValidByTokenHashAsync(string tokenHash, DateTime nowUtc)
            => _dbContext.RefreshTokens
                .Where(t => t.TokenHash == tokenHash && t.RevokedAtUtc == null && t.ExpiresAtUtc > nowUtc)
                .FirstOrDefaultAsync();

        public async Task RevokeAsync(Guid tokenId, DateTime revokedAtUtc, Guid? replacedByTokenId = null)
        {
            await _dbContext.RefreshTokens
                .Where(t => t.Id == tokenId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.RevokedAtUtc, revokedAtUtc)
                    .SetProperty(t => t.ReplacedByTokenId, replacedByTokenId));
        }

        public async Task RevokeAllForUserAsync(Guid userId, DateTime revokedAtUtc)
        {
            await _dbContext.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAtUtc, revokedAtUtc));
        }
    }
}
