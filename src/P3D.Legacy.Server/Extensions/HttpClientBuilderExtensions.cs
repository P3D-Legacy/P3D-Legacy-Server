using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Application.Utils;

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

            });

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