using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Domain.Options;
using P3D.Legacy.Server.Domain.Repositories;
using P3D.Legacy.Server.Domain.Services;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.Infrastructure.Repositories;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;
using P3D.Legacy.Server.Infrastructure.Repositories.Mutes;
using P3D.Legacy.Server.Infrastructure.Repositories.Permissions;
using P3D.Legacy.Server.Infrastructure.Repositories.Statistics;
using P3D.Legacy.Server.Infrastructure.Repositories.Users;

using System.Text.Json;

namespace P3D.Legacy.Server.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<JsonSerializerOptions>();
        services.AddOptions<PasswordOptions>();
        services.AddOptions<LockoutOptions>();

        services.AddHttpClient();

        services.AddTransient<IUserRepository, LiteDbUserRepository>();

        services.AddTransient<IMuteRepository, LiteDbMuteRepository>();

        services.AddTransient<IPermissionRepository, DefaultPermissionRepository>();
        services.AddTransient<P3DPermissionRepository>();
        services.AddTransient<LiteDbPermissionRepository>();

        services.AddTransient<IBanRepository, DefaultBanRepository>();
        services.AddTransient<P3DBanRepository>();
        services.AddTransient<LiteDbBanRepository>();

        services.AddTransient<Pokemon3DAPIClient>();

        /*
        services.AddTransient<IMonsterRepository, MonsterRepository>();
        services.AddTransient<PokeAPIMonsterRepository>();
        services.AddTransient<NopMonsterRepository>();
        */

        services.AddTransient<IStatisticsRepository, DefaultStatisticsRepository>();
        services.AddTransient<LiteDbStatisticsRepository>();

        services.AddTransient<IMonsterDataProvider, PokeAPIMonsterDataProvider>();

        return services;
    }
}