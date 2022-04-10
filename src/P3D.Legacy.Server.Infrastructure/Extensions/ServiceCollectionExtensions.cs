using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;
using P3D.Legacy.Server.Infrastructure.Repositories.Monsters;
using P3D.Legacy.Server.Infrastructure.Repositories.Permissions;
using P3D.Legacy.Server.Infrastructure.Repositories.Statistics;
using P3D.Legacy.Server.Infrastructure.Services.Bans;
using P3D.Legacy.Server.Infrastructure.Services.Mutes;
using P3D.Legacy.Server.Infrastructure.Services.Permissions;
using P3D.Legacy.Server.Infrastructure.Services.Statistics;
using P3D.Legacy.Server.Infrastructure.Services.Users;

using System.Text.Json;

namespace P3D.Legacy.Server.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JsonSerializerOptions>();
            services.AddOptions<PasswordOptions>();
            services.AddOptions<LockoutOptions>();

            services.AddHttpClient();

            services.AddTransient<IUserManager, LiteDbUserManager>();

            services.AddTransient<IMuteManager, LiteDbMuteManager>();

            services.AddTransient<IPermissionManager, DefaultPermissionManager>();
            services.AddTransient<P3DPermissionRepository>();
            services.AddTransient<LiteDbPermissionRepository>();

            services.AddTransient<IBanManager, DefaultBanManager>();
            services.AddTransient<P3DBanRepository>();
            services.AddTransient<LiteDbBanRepository>();

            services.AddTransient<IMonsterRepository, MonsterRepository>();
            services.AddTransient<PokeAPIMonsterRepository>();
            services.AddTransient<NopMonsterRepository>();

            services.AddTransient<IStatisticsManager, DefaultStatisticsManager>();
            services.AddTransient<LiteDbStatisticsRepository>();

            return services;
        }
    }
}