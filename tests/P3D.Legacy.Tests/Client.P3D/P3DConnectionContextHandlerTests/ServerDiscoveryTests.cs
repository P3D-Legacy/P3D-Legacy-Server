﻿using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

using P3D.Legacy.Server.Client.P3D;
using P3D.Legacy.Server.Client.P3D.Packets.Client;
using P3D.Legacy.Server.Client.P3D.Packets.Server;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Options;
using P3D.Legacy.Server.Domain.Queries;
using P3D.Legacy.Server.Domain.Queries.Options;
using P3D.Legacy.Server.Domain.Queries.Player;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests.Client.P3D.P3DConnectionContextHandlerTests;

internal sealed class ServerDiscoveryTests : BaseTests
{
    private class QueryDispatcherMock : IQueryDispatcher
    {
        public Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken ct)
        {
            switch (query)
            {
                case GetPlayersInitializedQuery when ImmutableArray.Create<IPlayer>(IPlayer.Server) is TQueryResult result:
                    return Task.FromResult(result);

                case GetServerOptionsQuery when new ServerOptions { MaxPlayers = 0 } is TQueryResult result:
                    return Task.FromResult(result);

                default:
                    ClassicAssert.Fail($"Missing handling of Query {query.GetType()}");
                    throw new AssertionException($"Missing handling of Query {query.GetType()}");
            }
        }
    }

    [Test]
    public async Task TestAsync() => await Common().Configure(static services =>
    {
        services.AddSingleton<ICommandDispatcher>(static _ => new Mock<ICommandDispatcher>(MockBehavior.Strict).Object);
        services.AddSingleton<IQueryDispatcher, QueryDispatcherMock>();
        services.AddSingleton<IEventDispatcher>(static _ => new Mock<IEventDispatcher>(MockBehavior.Strict).Object);
    }).DoTestAsync(static async sp =>
    {
        var connectionFactory = sp.GetRequiredService<IConnectionFactory>();

        var connection = (DefaultConnectionContext) await connectionFactory.ConnectAsync(IPEndPoint.Parse("127.0.0.1:80"));
        var handler = sp.GetRequiredService<P3DConnectionHandler>();
        var connectionTask = handler.OnConnectedAsync(connection);

        var protocol = sp.GetRequiredService<P3DProtocol>();
        var reader = new ProtocolReader(connection.Application!.Input);
        var writer = new ProtocolWriter(connection.Application!.Output);

        await writer.WriteAsync(protocol, new ServerDataRequestPacket());
        var response = await reader.ReadAsync(protocol);
        reader.Advance();
        ClassicAssert.IsFalse(response.IsCanceled);
        ClassicAssert.IsFalse(response.IsCompleted);
        ClassicAssert.NotNull(response.Message);
        ClassicAssert.IsInstanceOf<ServerInfoDataPacket>(response.Message);

        var serverInfoDataPacket = (ServerInfoDataPacket) response.Message!;
        ClassicAssert.NotNull(serverInfoDataPacket.PlayerNames.Single(static x => string.Equals(x, IPlayer.Server.Name, StringComparison.Ordinal)));
        ClassicAssert.AreEqual(1, serverInfoDataPacket.CurrentPlayers);
        ClassicAssert.AreEqual(0, serverInfoDataPacket.MaxPlayers);

        await connectionTask.WaitAsync(TimeSpan.FromMilliseconds(1000));
    });
}