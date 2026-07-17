using Microsoft.Extensions.DependencyInjection;
using StaffingManagementSystem.Repositories.Interfaces;

namespace StaffingManagementSystem.Repositories.Extensions
{
    /// <summary>
    /// Registers Repository-layer services with the DI container.
    /// </summary>
    public static class RepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<ICandidateAttachmentRepository, CandidateAttachmentRepository>();

            return services;
        }
    }
}
