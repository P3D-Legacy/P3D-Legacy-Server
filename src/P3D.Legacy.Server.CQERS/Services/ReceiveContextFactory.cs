﻿using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.Domain.Events;

using System;

namespace P3D.Legacy.Server.CQERS.Services;

public sealed class ReceiveContextFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ReceiveContextFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IReceiveContectWithPublisher<TEvent> Create<TEvent>(TEvent @event) where TEvent : IEvent =>
        ActivatorUtilities.CreateInstance<ReceiveContext<TEvent>>(_serviceProvider, @event);
}