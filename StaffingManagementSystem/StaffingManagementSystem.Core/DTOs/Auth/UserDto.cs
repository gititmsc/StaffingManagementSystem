namespace StaffingManagementSystem.Core.DTOs.Auth
{
    /// <summary>
    /// Safe, public projection of a <see cref="Entities.User"/> returned to clients.
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
