using Microsoft.EntityFrameworkCore;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Infrastructure.Persistence;
using StaffingManagementSystem.Repositories.Interfaces;

namespace StaffingManagementSystem.Repositories
{
    /// <inheritdoc cref="IUserRepository"/>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetByEmailAsync(string email)
            => _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task UpdateLastLoginAsync(Guid userId, DateTime loginAtUtc)
        {
            await _dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastLoginAtUtc, loginAtUtc));
        }

        public async Task UpdatePasswordHashAsync(Guid userId, string passwordHash)
        {
            await _dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.PasswordHash, passwordHash)
                    .SetProperty(u => u.UpdatedAtUtc, DateTime.UtcNow));
        }
    }
}
