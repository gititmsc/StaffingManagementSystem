using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for <see cref="User"/> records.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task UpdateLastLoginAsync(Guid userId, DateTime loginAtUtc);
    }
}
