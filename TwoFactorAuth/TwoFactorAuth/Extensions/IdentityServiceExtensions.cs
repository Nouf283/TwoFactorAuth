using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwoFactorAuth.Infrustracture.Identity;

namespace TwoFactorAuth.Extensions
{
    public class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}
