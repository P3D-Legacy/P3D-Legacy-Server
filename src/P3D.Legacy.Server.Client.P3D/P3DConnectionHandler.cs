﻿using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Application.Services;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    internal sealed class P3DConnectionHandler : ConnectionHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public P3DConnectionHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            await using var connectionScope = _serviceScopeFactory.CreateAsyncScope();

            var connectionContextHandlerFactory = connectionScope.ServiceProvider.GetRequiredService<ConnectionContextHandlerFactory>();
            using var connectionContextHandler = await connectionContextHandlerFactory.CreateAsync<P3DConnectionContextHandler>(connection);

            try
            {
                await connectionContextHandler.ListenAsync();
            }
            catch (Exception e) when (e is TaskCanceledException or OperationCanceledException) { }
        }
    }
}