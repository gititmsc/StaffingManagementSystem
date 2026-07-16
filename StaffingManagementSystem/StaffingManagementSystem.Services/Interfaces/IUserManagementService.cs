using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Users;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Business logic contract for administering user accounts and roles.
    /// </summary>
    public interface IUserManagementService
    {
        Task<ApiResponse<List<UserListItemDto>>> GetAllUsersAsync();

        Task<ApiResponse<UserListItemDto>> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Creates a user with no usable password and emails them a "set up your password" link.
        /// </summary>
        Task<ApiResponse<UserListItemDto>> CreateUserAsync(CreateUserRequestDto request);

        Task<ApiResponse<UserListItemDto>> UpdateUserAsync(Guid id, UpdateUserRequestDto request);

        /// <summary><paramref name="actingUserId"/> may not deactivate their own account.</summary>
        Task<ApiResponse<object>> SetUserStatusAsync(Guid id, bool isActive, Guid actingUserId);

        /// <summary><paramref name="actingUserId"/> may not delete their own account.</summary>
        Task<ApiResponse<object>> DeleteUserAsync(Guid id, Guid actingUserId);
    }
}
