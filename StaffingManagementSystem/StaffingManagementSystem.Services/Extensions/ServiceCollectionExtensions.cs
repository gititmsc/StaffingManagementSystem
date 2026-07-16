using Microsoft.Extensions.DependencyInjection;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Services.Extensions
{
    /// <summary>
    /// Registers Service-layer (business logic) services with the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
