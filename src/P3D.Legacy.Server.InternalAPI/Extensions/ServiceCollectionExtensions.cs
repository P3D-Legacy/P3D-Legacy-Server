using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.InternalAPI.Controllers;
using P3D.Legacy.Server.InternalAPI.Options;

using System;
using System.Text;

namespace P3D.Legacy.Server.InternalAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInternalAPIMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            return services;
        }
        public static IServiceCollection AddInternalAPI(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddAuthentication(options =>
                {
                    options.AddScheme<GameJoltAuthenticationHandler>("gamejolt", "GameJolt");
                })
                .AddJwtBearer(options =>
                {
                    var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = false,
                        ValidateActor = false,
                        ValidateTokenReplay = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SignKey)),
                        TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.EncryptionKey)),
                        ClockSkew = TimeSpan.FromMinutes(5),
                    };
                });

            return services;
        }
    }
}