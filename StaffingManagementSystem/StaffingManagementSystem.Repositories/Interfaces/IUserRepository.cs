using StaffingManagementSystem.Core.Entities;

namespace StaffingManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for <see cref="User"/> records.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>Returns the active, non-deleted user with this email, or null. Used for login.</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Returns the non-deleted user with this id, or null.</summary>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>Returns every non-deleted user, newest first.</summary>
        Task<List<User>> GetAllAsync();

        /// <summary>True if a non-deleted user with this email exists (case-insensitive), optionally excluding one id.</summary>
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);

        Task CreateAsync(User user);

        /// <summary>Updates the editable profile fields (name, phone, department, role) of an existing user.</summary>
        Task UpdateAsync(User user);

        Task UpdateLastLoginAsync(Guid userId, DateTime loginAtUtc);

        Task UpdatePasswordHashAsync(Guid userId, string passwordHash);

        Task SetActiveStatusAsync(Guid userId, bool isActive);

        /// <summary>Soft-deletes a user: hidden from all user-management views but retained for history.</summary>
        Task SoftDeleteAsync(Guid userId);
    }
}
