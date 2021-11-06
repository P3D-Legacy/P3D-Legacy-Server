using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Application.Utils;

using System;

namespace P3D.Legacy.Server.Extensions
{
    internal static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddPolly(this IHttpClientBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddPolicyHandler(PollyUtils.PolicySelector);
        }
    }
}