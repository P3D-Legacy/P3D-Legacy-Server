using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Application.Queries.Options;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.Client.P3D;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests.Client.P3D.P3DConnectionContextHandlerTests
{
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
                        Assert.Fail($"Missing handling of Query {query.GetType()}");
                        throw new AssertionException($"Missing handling of Query {query.GetType()}");
                }
            }
        }

        [Test]
        public async Task TestAsync()
        {
            await using var testService = Commnon().Configure(services =>
            {
                services.AddSingleton<ICommandDispatcher>(_ => new Mock<ICommandDispatcher>(MockBehavior.Strict).Object);
                services.AddSingleton<IQueryDispatcher, QueryDispatcherMock>();
                services.AddSingleton<IEventDispatcher>(_ => new Mock<IEventDispatcher>(MockBehavior.Strict).Object);
            });

            await testService.DoTestAsync(async serviceProvider =>
            {
                var connectionFactory = serviceProvider.GetRequiredService<IConnectionFactory>();

                var connection = (DefaultConnectionContext) await connectionFactory.ConnectAsync(IPEndPoint.Parse("127.0.0.1:80"));
                var handler = serviceProvider.GetRequiredService<P3DConnectionHandler>();
                var connectionTask = handler.OnConnectedAsync(connection);

                var protocol = serviceProvider.GetRequiredService<P3DProtocol>();
                var reader = new ProtocolReader(connection.Application!.Input);
                var writer = new ProtocolWriter(connection.Application!.Output);

                await writer.WriteAsync(protocol, new ServerDataRequestPacket());
                var response = await reader.ReadAsync(protocol);
                reader.Advance();

                Assert.NotNull(response);
                Assert.IsFalse(response.IsCanceled);
                Assert.IsFalse(response.IsCompleted);
                Assert.NotNull(response.Message);
                Assert.IsInstanceOf<ServerInfoDataPacket>(response.Message);

                var serverInfoDataPacket = (ServerInfoDataPacket) response.Message!;
                Assert.NotNull(serverInfoDataPacket.PlayerNames.Single(x => x == IPlayer.Server.Name));
                Assert.AreEqual(1, serverInfoDataPacket.CurrentPlayers);
                Assert.AreEqual(0, serverInfoDataPacket.MaxPlayers);

                await connectionTask.WaitAsync(TimeSpan.FromMilliseconds(1000));
            });
        }
    }
}
