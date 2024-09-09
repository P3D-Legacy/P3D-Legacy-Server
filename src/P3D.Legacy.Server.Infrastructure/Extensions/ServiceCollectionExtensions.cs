using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Domain.Options;
using P3D.Legacy.Server.Domain.Repositories;
using P3D.Legacy.Server.Domain.Services;
using P3D.Legacy.Server.Infrastructure.Configuration;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.Infrastructure.Repositories;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;
using P3D.Legacy.Server.Infrastructure.Repositories.Mutes;
using P3D.Legacy.Server.Infrastructure.Repositories.Permissions;
using P3D.Legacy.Server.Infrastructure.Repositories.Statistics;
using P3D.Legacy.Server.Infrastructure.Repositories.Users;

using System;
using System.Net.Http;
using System.Reflection;
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

        services.AddTransient<IDynamicConfigurationProviderManager, DynamicConfigurationProviderManager>();

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

        var assemblyName = Assembly.GetEntryAssembly()?.GetName();
        var userAgent = $"{assemblyName?.Name ?? "ERROR"} v{assemblyName?.Version?.ToString() ?? "ERROR"}";
        services.AddHttpClient("pokeapi").ConfigureHttpClient((sp, client) =>
        {
            client.BaseAddress = new Uri("https+http://pokeapi");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddServiceDiscovery();
        services.AddTransient<IMonsterDataProvider, PokeAPIMonsterDataProvider>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

#pragma warning disable IDISP001
                var httpClient = httpClientFactory.CreateClient("pokeapi");
                var graphQlClient = new GraphQLHttpClient(new GraphQLHttpClientOptions(), new SystemTextJsonSerializer(), httpClient);
#pragma warning restore IDISP001
                return ActivatorUtilities.CreateInstance<PokeAPIMonsterDataProvider>(sp, graphQlClient);
            }
        );

        return services;
    }
}