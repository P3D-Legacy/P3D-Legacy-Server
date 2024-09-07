using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework;

public static partial class ServerBuilderExtensions
{
    public static ServerBuilder Listen<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTransport>(this ServerBuilder builder, EndPoint endPoint, Action<IConnectionBuilder> configure) where TTransport : IConnectionListenerFactory
    {
        return builder.Listen(endPoint, ActivatorUtilities.CreateInstance<TTransport>(builder.ApplicationServices), configure);
    }

    public static ServerBuilder Listen(this ServerBuilder builder, EndPoint endPoint, IConnectionListenerFactory connectionListenerFactory, Action<IConnectionBuilder> configure)
    {
        var connectionBuilder = new ConnectionBuilder(builder.ApplicationServices);
        configure(connectionBuilder);
        builder.Bindings.Add(new EndPointBinding(endPoint, connectionBuilder.Build(), connectionListenerFactory));
        return builder;
    }
}