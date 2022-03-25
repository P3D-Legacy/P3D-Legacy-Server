using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Services;

using System;

namespace P3D.Legacy.Server.Abstractions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidatedOptions<TOptions, TOptionsValidator>(this IServiceCollection services, IConfiguration configuration)
            where TOptions : class where TOptionsValidator : class, IValidator<TOptions>
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddHostedService<OptionValidationService>();

            services.AddOptions<TOptions>()
                .Bind(configuration)
                .ValidateViaFluent<TOptions, TOptionsValidator>()
                .ValidateViaHostManager();

            return services;
        }

        public static IServiceCollection AddValidatedOptionsWithHttp<TOptions, TOptionsValidator>(this IServiceCollection services, IConfiguration configuration, Action<IHttpClientBuilder>? httpClientBuilder = null)
            where TOptions : class where TOptionsValidator : class, IValidator<TOptions>
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddHostedService<OptionValidationService>();

            services.AddOptions<TOptions>()
                .Bind(configuration)
                .ValidateViaFluentWithHttp<TOptions, TOptionsValidator>(httpClientBuilder)
                .ValidateViaHostManager();

            return services;
        }
    }
}
