using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.Interfaces;
using StaffingManagementSystem.Infrastructure.Persistence;
using StaffingManagementSystem.Infrastructure.Security;

namespace StaffingManagementSystem.Infrastructure.Extensions
{
    /// <summary>
    /// Registers Infrastructure-layer services (DbContext, security utilities) with the DI container.
    /// </summary>
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("StaffingManagementSystemDb")));

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
