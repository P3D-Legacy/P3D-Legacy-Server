using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using P3D.Legacy.Server.InternalAPI.Options;

using System;
using System.Security.Cryptography;
using System.Text;

namespace P3D.Legacy.Server.InternalAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInternalAPI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();

                    var rsa = RSA.Create();
                    rsa.ImportFromPem(Encoding.UTF8.GetString(Convert.FromBase64String(jwtOptions.RsaPrivateKey)));

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = "P3D Legacy Server",
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidateActor = false,
                        ValidateTokenReplay = false,
                        IssuerSigningKey = new RsaSecurityKey(rsa),
                        ClockSkew = TimeSpan.FromMinutes(5),
                    };
                });

            return services;
        }
    }
}