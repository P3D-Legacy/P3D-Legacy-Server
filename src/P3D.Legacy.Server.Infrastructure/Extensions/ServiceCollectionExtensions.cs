using LiteDB.Identity.Database;
using LiteDB.Identity.Models;
using LiteDB.Identity.Stores;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Identity;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Infrastructure.Monsters;
using P3D.Legacy.Server.Infrastructure.Permissions;
using P3D.Legacy.Server.Infrastructure.Repositories;

using System;

namespace P3D.Legacy.Server.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            return services;
        }
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var validationEnabled = configuration["Server:ValidationEnabled"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            var isOfficial = configuration["Server:IsOfficial"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

            if (isOfficial)
            {
                services.AddTransient<IPermissionRepository, P3DPermissionRepository>();
                services.AddTransient<IBanRepository, LiteDbBanRepository>(); // TODO
            }
            else
            {
                services.AddTransient<IPermissionRepository, DefaultPermissionRepository>();
                services.AddTransient<IBanRepository, LiteDbBanRepository>();
            }

            if (validationEnabled)
            {
                services.AddTransient<IMonsterRepository, PokeAPIMonsterRepository>();
            }
            else
            {
                services.AddTransient<IMonsterRepository, NopMonsterRepository>();
            }

            services.AddScoped<ILiteDbIdentityContext, LiteDbIdentityContext>(c => new LiteDbIdentityContext("Filename=./database.litedb;Connection=Shared"));

            services.AddIdentity<ServerIdentity, ServerRole>()
                .AddUserStore<UserStore<ServerIdentity, ServerRole, ServerIdentityRole, ServerIdentityClaim, ServerIdentityLogin, ServerIdentityToken>>()
                .AddRoleStore<RoleStore<ServerRole, ServerRoleClaim>>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}