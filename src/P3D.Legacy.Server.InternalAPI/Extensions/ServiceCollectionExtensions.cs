using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.InternalAPI.Options;

namespace P3D.Legacy.Server.InternalAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInternalAPI(this IServiceCollection services)
        {
            services.AddAuthentication().AddJwtBearer();
            services.ConfigureOptions<JwtBearerOptionsConfigure>();

            return services;
        }
    }
}