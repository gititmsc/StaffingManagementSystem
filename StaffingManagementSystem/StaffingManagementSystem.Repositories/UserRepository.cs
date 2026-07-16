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
            => _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

        public Task<User?> GetByIdAsync(Guid id)
            => _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        public Task<List<User>> GetAllAsync()
            => _dbContext.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToListAsync();

        public Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
            => _dbContext.Users.AnyAsync(u =>
                !u.IsDeleted &&
                u.Email == email &&
                (excludeUserId == null || u.Id != excludeUserId));

        public async Task CreateAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            await _dbContext.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.FirstName, user.FirstName)
                    .SetProperty(u => u.LastName, user.LastName)
                    .SetProperty(u => u.PhoneNumber, user.PhoneNumber)
                    .SetProperty(u => u.Department, user.Department)
                    .SetProperty(u => u.Role, user.Role)
                    .SetProperty(u => u.UpdatedAtUtc, DateTime.UtcNow));
        }

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

        public async Task SetActiveStatusAsync(Guid userId, bool isActive)
        {
            await _dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsActive, isActive)
                    .SetProperty(u => u.UpdatedAtUtc, DateTime.UtcNow));
        }

        public async Task SoftDeleteAsync(Guid userId)
        {
            await _dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsDeleted, true)
                    .SetProperty(u => u.IsActive, false)
                    .SetProperty(u => u.UpdatedAtUtc, DateTime.UtcNow));
        }
    }
}
