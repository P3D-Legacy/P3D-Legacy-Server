using CorrelationId;
using CorrelationId.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Application.Utils;
using P3D.Legacy.Server.Utils;

using System;

namespace P3D.Legacy.Server.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder GenerateCorrelationId(this IHttpClientBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureHttpClient((sp, client) =>
            {
                var correlationIdOptions = sp.GetRequiredService<IOptions<CorrelationIdOptions>>().Value;
                var correlationIdProvider = sp.GetRequiredService<ICorrelationIdProvider>();
                client.DefaultRequestHeaders.Add(correlationIdOptions.RequestHeader, correlationIdProvider.GenerateCorrelationId(null));
            });

            return builder;
        }

        public static IHttpClientBuilder AddCorrelationIdOverrideForwarding(this IHttpClientBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddTransient<CorrelationIdOverrideHandler>();
            builder.AddHttpMessageHandler<CorrelationIdOverrideHandler>();
            return builder;
        }

        public static IHttpClientBuilder AddPolly(this IHttpClientBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddPolicyHandler(PollyUtils.PolicySelector);
        }
    }
}