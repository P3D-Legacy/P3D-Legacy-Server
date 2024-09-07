using System;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework.Infrastructure;

internal class EmptyServiceProvider : IServiceProvider
{
    public static IServiceProvider Instance { get; } = new EmptyServiceProvider();

    public object? GetService(Type serviceType) => null;
}