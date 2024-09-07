using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework;

internal static class ServiceProviderExtensions
{
    internal static ILoggerFactory GetLoggerFactory(this IServiceProvider serviceProvider) =>
        serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory ?? NullLoggerFactory.Instance;
}