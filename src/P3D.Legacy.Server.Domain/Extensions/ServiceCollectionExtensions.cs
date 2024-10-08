﻿using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Queries;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace P3D.Legacy.Server.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] TEventHandler>(this IServiceCollection services)
        where TEventHandler : IEventHandler
    {
        var @typeof = typeof(TEventHandler);
        var @typeofEventInterfaces = @typeof.GetInterfaces().Where(static x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        foreach (var eventHandlerInterface in @typeofEventInterfaces)
        {
            var eventType = eventHandlerInterface.GenericTypeArguments[0];
            var baseType = typeof(IEventHandler<>).MakeGenericType(eventType);

            services.Add(ServiceDescriptor.Transient(baseType, @typeof));
        }

        return services;
    }
    public static IServiceCollection AddEventHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TEventHandler>(this IServiceCollection services, Func<IServiceProvider, TEventHandler?> factory)
        where TEventHandler : IEventHandler
    {
        var @typeof = typeof(TEventHandler);
        var @typeofEventInterfaces = @typeof.GetInterfaces().Where(static x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        foreach (var eventHandlerInterface in @typeofEventInterfaces)
        {
            var genericType = eventHandlerInterface.GenericTypeArguments[0];
            var openMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddEventHandler), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var method = openMethod?.MakeGenericMethod(@typeof, genericType);
            method?.Invoke(null, [services, factory]);
        }

        return services;
    }
    public static IServiceCollection AddEventHandlers<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TEventHandler>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<TEventHandler>> factory)
        where TEventHandler : IEventHandler
    {
        var @typeof = typeof(TEventHandler);
        var @typeofEventInterfaces = @typeof.GetInterfaces().Where(static x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        foreach (var eventHandlerInterface in @typeofEventInterfaces)
        {
            var genericType = eventHandlerInterface.GenericTypeArguments[0];
            var openMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddEventHandlers), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var method = openMethod?.MakeGenericMethod(@typeof, genericType);
            method?.Invoke(null, [services, factory]);
        }

        return services;
    }
    private static IServiceCollection AddEventHandler<TEventHandler, TEvent>(this IServiceCollection services, Func<IServiceProvider, TEventHandler?> factory)
        where TEvent : IEvent
        where TEventHandler : IEventHandler<TEvent>
    {
        services.Add(ServiceDescriptor.Transient<IEventHandler<TEvent>>(sp => new EventHandlerWrapper<TEventHandler, TEvent>(sp, factory)));

        return services;
    }
    private static IServiceCollection AddEventHandlers<TEventHandler, TEvent>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<TEventHandler>> factory)
        where TEvent : IEvent
        where TEventHandler : IEventHandler<TEvent>
    {
        services.Add(ServiceDescriptor.Transient<IEventHandler<TEvent>>(sp => new EventHandlerEnumerableWrapper<TEventHandler, TEvent>(sp, factory)));

        return services;
    }

    public static IServiceCollection AddCommandHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] TCommandHandler>(this IServiceCollection services)
        where TCommandHandler : class, ICommandHandler
    {
        var @typeof = typeof(TCommandHandler);
        var @typeofCommandHandlerInterface = @typeof.GetInterfaces().Where(static x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
        foreach (var commandHandlerInterface in @typeofCommandHandlerInterface)
        {
            var commandType = commandHandlerInterface.GenericTypeArguments[0];
            var baseType = typeof(ICommandHandler<>).MakeGenericType(commandType);

            services.Add(ServiceDescriptor.Transient(baseType, @typeof));
        }

        return services;
    }
    public static IServiceCollection AddCommandHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TCommandHandler>(this IServiceCollection services, Func<IServiceProvider, TCommandHandler> factory)
        where TCommandHandler : class, ICommandHandler
    {
        var @typeof = typeof(TCommandHandler);
        var @typeofCommandHandlerInterface = @typeof.GetInterfaces().Where(static x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
        foreach (var commandHandlerInterface in @typeofCommandHandlerInterface)
        {
            var commandType = commandHandlerInterface.GenericTypeArguments[0];
            var baseType = typeof(ICommandHandler<>).MakeGenericType(commandType);

            services.Add(ServiceDescriptor.Transient(baseType, factory));
        }

        return services;
    }

    public static IServiceCollection AddQueryHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] TQueryHandler>(this IServiceCollection services)
        where TQueryHandler : class, IQueryHandler
    {
        var @typeof = typeof(TQueryHandler);
        var @typeofQueryHandlerInterface = @typeof.GetInterfaces().Where(static x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
        foreach (var queryHandlerInterface in @typeofQueryHandlerInterface)
        {
            var queryType = queryHandlerInterface.GenericTypeArguments[0];
            var queryResultType = queryHandlerInterface.GenericTypeArguments[1];
            var baseType = typeof(IQueryHandler<,>).MakeGenericType(queryType, queryResultType);

            services.Add(ServiceDescriptor.Transient(baseType, @typeof));
        }

        return services;
    }
    public static IServiceCollection AddQueryHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TQueryHandler>(this IServiceCollection services, Func<IServiceProvider, TQueryHandler> factory)
        where TQueryHandler : class, IQueryHandler
    {
        var @typeof = typeof(TQueryHandler);
        var @typeofQueryHandlerInterface = @typeof.GetInterfaces().Where(static x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
        foreach (var queryHandlerInterface in @typeofQueryHandlerInterface)
        {
            var queryType = queryHandlerInterface.GenericTypeArguments[0];
            var queryResultType = queryHandlerInterface.GenericTypeArguments[1];
            var baseType = typeof(IQueryHandler<,>).MakeGenericType(queryType, queryResultType);

            services.Add(ServiceDescriptor.Transient(baseType, factory));
        }

        return services;
    }
}