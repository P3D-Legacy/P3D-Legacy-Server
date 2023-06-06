using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Queries.Options;
using P3D.Legacy.Server.Application.Queries.Player;
using P3D.Legacy.Server.Application.Queries.World;
using P3D.Legacy.Server.Client.P3D;
using P3D.Legacy.Server.Client.P3D.Packets.Chat;
using P3D.Legacy.Server.Client.P3D.Packets.Common;
using P3D.Legacy.Server.Client.P3D.Packets.Server;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests.Client.P3D.P3DConnectionContextHandlerTests
{
    internal sealed class ChatTests : BaseTests
    {
        private class CommandDispatcherMock : ICommandDispatcher
        {
            private readonly IEventDispatcher _eventDispatcher;

            public CommandDispatcherMock(IEventDispatcher eventDispatcher)
            {
                _eventDispatcher = eventDispatcher;
            }

            public async Task<CommandResult> DispatchAsync<TCommand>(TCommand rawCommand, CancellationToken ct) where TCommand : ICommand
            {
                switch (rawCommand)
                {
                    case PlayerInitializingCommand command:
                        await command.Player.AssignIdAsync(PlayerId.FromGameJolt(GameJoltId.FromNumber(0)), ct);
                        return CommandResult.Success;
                    case PlayerAuthenticateGameJoltCommand:
                        return CommandResult.Success;
                    case PlayerReadyCommand command:
                        await _eventDispatcher.DispatchAsync(new PlayerJoinedEvent(command.Player), ct);
                        return CommandResult.Success;
                    case PlayerFinalizingCommand:
                        return CommandResult.Success;
                    default:
                        Assert.Fail($"Missing handling of Command {typeof(TCommand)}");
                        throw new AssertionException($"Missing handling of Command {typeof(TCommand)}");
                }
            }
        }
        private class QueryDispatcherMock : IQueryDispatcher
        {
            public Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken ct)
            {
                switch (query)
                {
                    case GetPlayersInitializedQuery when ImmutableArray<IPlayer>.Empty is TQueryResult result:
                        return Task.FromResult(result);
                    case GetServerOptionsQuery when new ServerOptions { MaxPlayers = 1 } is TQueryResult result:
                        return Task.FromResult(result);
                    case GetWorldStateQuery when new WorldState(TimeSpan.Zero, default, default) is TQueryResult result:
                        return Task.FromResult(result);
                    default:
                        Assert.Fail($"Missing handling of Query {query.GetType()}");
                        throw new AssertionException($"Missing handling of Query {query.GetType()}");
                }
            }
        }
        private class EventDispatcherMock : IEventDispatcher
        {
            public DispatchStrategy DefaultStrategy { get; set; } = DispatchStrategy.ParallelWhenAll;

            private readonly List<IPlayer> _list;
            private readonly TaskCompletionSource<string> _lock;

            public EventDispatcherMock(List<IPlayer> list, TaskCompletionSource<string> @lock)
            {
                _list = list;
                _lock = @lock;
            }

            public Task DispatchAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IEvent => DispatchAsync(@event, DefaultStrategy, ct);
            public async Task DispatchAsync<TEvent>(TEvent rawEvent, DispatchStrategy dispatchStrategy, CancellationToken ct) where TEvent : IEvent
            {
                switch (rawEvent)
                {
                    case PlayerUpdatedStateEvent:
                        return;
                    case PlayerJoinedEvent notification:
                        _list.Add(notification.Player);
                        return;
                    case PlayerSentGlobalMessageEvent notification:
                        _lock.SetResult(notification.Message);
                        foreach (var eventHandler in _list.OfType<IEventHandler<PlayerSentGlobalMessageEvent>>())
                            await eventHandler.HandleAsync(new NullReceiveContext<PlayerSentGlobalMessageEvent>(notification), ct);
                        return;
                    default:
                        Assert.Fail($"Missing handling of Event {typeof(TEvent)}");
                        throw new AssertionException($"Missing handling of Event {typeof(TEvent)}");
                }
            }
        }

        [Test]
        public async Task TestAsync() => await Common().Configure(static services =>
        {
            services.AddSingleton<ICommandDispatcher, CommandDispatcherMock>();
            services.AddSingleton<IQueryDispatcher, QueryDispatcherMock>();
            services.AddSingleton<IEventDispatcher, EventDispatcherMock>();

            services.AddSingleton<List<IPlayer>>();

            services.AddSingleton<TaskCompletionSource<string>>();
        }).DoTestAsync(static async serviceProvider =>
        {
            var connectionFactory = serviceProvider.GetRequiredService<IConnectionFactory>();

            var @lock = serviceProvider.GetRequiredService<TaskCompletionSource<string>>();
            var list = serviceProvider.GetRequiredService<List<IPlayer>>();

            var connection = (DefaultConnectionContext) await connectionFactory.ConnectAsync(IPEndPoint.Parse("127.0.0.1:80"));
            var handler = serviceProvider.GetRequiredService<P3DConnectionHandler>();
            var connectionTask = handler.OnConnectedAsync(connection);

            var protocol = serviceProvider.GetRequiredService<P3DProtocol>();
            var reader = new ProtocolReader(connection.Application!.Input);
            var writer = new ProtocolWriter(connection.Application!.Output);

            var gameData = new GameDataPacket
            {
                GameMode = "state.GameMode",
                IsGameJoltPlayer = false,
                GameJoltId = 0,
                DecimalSeparator = '.',
                Name = "player.Name",
                LevelFile = "state.LevelFile",
                Position = Vector3.Zero,
                Facing = 0,
                Moving = false,
                Skin = "state.Skin",
                BusyType = "state.BusyType",
                MonsterVisible = false,
                MonsterPosition = Vector3.Zero,
                MonsterSkin = "state.MonsterSkin",
                MonsterFacing = 0
            };
            await writer.WriteAsync(protocol, gameData);

            var idPacket = await reader.ReadAsync(protocol);
            reader.Advance();
            Assert.IsFalse(idPacket.IsCanceled);
            Assert.IsFalse(idPacket.IsCompleted);
            Assert.NotNull(idPacket.Message);
            Assert.IsInstanceOf<IdPacket>(idPacket.Message);

            var origin = idPacket.Message!.Origin;

            var worldDataPacket = await reader.ReadAsync(protocol);
            reader.Advance();
            Assert.IsFalse(worldDataPacket.IsCanceled);
            Assert.IsFalse(worldDataPacket.IsCompleted);
            Assert.NotNull(worldDataPacket.Message);
            Assert.IsInstanceOf<WorldDataPacket>(worldDataPacket.Message);

            var gameDataPacket = await reader.ReadAsync(protocol);
            reader.Advance();
            Assert.IsFalse(gameDataPacket.IsCanceled);
            Assert.IsFalse(gameDataPacket.IsCompleted);
            Assert.NotNull(gameDataPacket.Message);
            Assert.IsInstanceOf<GameDataPacket>(gameDataPacket.Message);

            const string message = "Test123";

            await writer.WriteAsync(protocol, new ChatMessageGlobalPacket { Origin = origin, Message = message });

            await @lock.Task.WaitAsync(TimeSpan.FromMilliseconds(1000));

            var chatMessage = await reader.ReadAsync(protocol);
            reader.Advance();
            Assert.IsFalse(chatMessage.IsCanceled);
            Assert.IsFalse(chatMessage.IsCompleted);
            Assert.NotNull(chatMessage.Message);
            Assert.IsInstanceOf<ChatMessageGlobalPacket>(chatMessage.Message);

            foreach (var player in list)
                await player.KickAsync("", CancellationToken.None);

            await connectionTask.WaitAsync(TimeSpan.FromMilliseconds(1000));
        });
    }
}