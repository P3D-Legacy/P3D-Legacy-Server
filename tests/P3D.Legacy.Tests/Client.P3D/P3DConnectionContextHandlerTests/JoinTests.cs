﻿using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using NUnit.Framework.Legacy;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Client.P3D;
using P3D.Legacy.Server.Client.P3D.Packets;
using P3D.Legacy.Server.Client.P3D.Packets.Common;
using P3D.Legacy.Server.Client.P3D.Packets.Server;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;
using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Commands.Player;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.Player;
using P3D.Legacy.Server.Domain.Extensions;
using P3D.Legacy.Server.Domain.Options;
using P3D.Legacy.Server.Domain.Queries;
using P3D.Legacy.Server.Domain.Queries.Options;
using P3D.Legacy.Server.Domain.Queries.Player;
using P3D.Legacy.Server.Domain.Queries.World;

using System;
using System.Collections.Immutable;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests.Client.P3D.P3DConnectionContextHandlerTests;

internal sealed class JoinTests : BaseTests
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
                    await command.Player.AssignIdAsync(PlayerId.FromGameJolt(GameJoltId.From(0)), ct);
                    return CommandResult.Success;

                case PlayerAuthenticateGameJoltCommand:
                    return CommandResult.Success;

                case PlayerReadyCommand command:
                    await _eventDispatcher.DispatchAsync(new PlayerJoinedEvent(command.Player), ct);
                    return CommandResult.Success;

                case PlayerFinalizingCommand:
                    return CommandResult.Success;

                default:
                    ClassicAssert.Fail($"Missing handling of Command {typeof(TCommand)}");
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
                    ClassicAssert.Fail($"Missing handling of Query {query.GetType()}");
                    throw new AssertionException($"Missing handling of Query {query.GetType()}");
            }
        }
    }
    private class EventDispatcherMock : IEventDispatcher
    {
        public DispatchStrategy DefaultStrategy { get; set; } = DispatchStrategy.ParallelWhenAll;

        public async Task DispatchAsync<TEvent>(TEvent rawEvent, DispatchStrategy dispatchStrategy, bool trace, CancellationToken ct) where TEvent : IEvent
        {
            switch (rawEvent)
            {
                case PlayerUpdatedStateEvent:
                    return;

                case PlayerJoinedEvent notification:
                    await notification.Player.KickAsync("", ct);
                    return;

                default:
                    ClassicAssert.Fail($"Missing handling of Event {typeof(TEvent)}");
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
    }).DoTestAsync(static async serviceProvider =>
    {
        var connectionFactory = serviceProvider.GetRequiredService<IConnectionFactory>();

        var connection = (DefaultConnectionContext) await connectionFactory.ConnectAsync(IPEndPoint.Parse("127.0.0.1:80"), CancellationToken.None);
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
            MonsterFacing = 0,
        };
        await writer.WriteAsync(protocol, gameData);

        var idPacket = await reader.ReadAsync(protocol);
        reader.Advance();
        ClassicAssert.IsFalse(idPacket.IsCanceled);
        ClassicAssert.IsFalse(idPacket.IsCompleted);
        ClassicAssert.NotNull(idPacket.Message);
        ClassicAssert.IsInstanceOf<IdPacket>(idPacket.Message);

        var worldDataPacket = await reader.ReadAsync(protocol);
        reader.Advance();
        ClassicAssert.IsFalse(worldDataPacket.IsCanceled);
        ClassicAssert.IsFalse(worldDataPacket.IsCompleted);
        ClassicAssert.NotNull(worldDataPacket.Message);
        ClassicAssert.IsInstanceOf<WorldDataPacket>(worldDataPacket.Message);

        var kickPacket = await reader.ReadAsync(protocol);
        reader.Advance();
        ClassicAssert.IsFalse(kickPacket.IsCanceled);
        ClassicAssert.IsFalse(kickPacket.IsCompleted);
        ClassicAssert.NotNull(kickPacket.Message);
        ClassicAssert.IsInstanceOf<KickedPacket>(kickPacket.Message);

        var emptyPacket = await await Task.WhenAny(reader.ReadAsync(protocol).AsTask(), WaitNullPacketAsync(100));
        ClassicAssert.IsFalse(emptyPacket.IsCanceled);
        ClassicAssert.IsFalse(emptyPacket.IsCompleted);
        ClassicAssert.Null(emptyPacket.Message);

        await connectionTask.WaitAsync(TimeSpan.FromMilliseconds(1000));
    });

    private static async Task<ProtocolReadResult<P3DPacket?>> WaitNullPacketAsync(int delayMs)
    {
        await Task.Delay(delayMs);
        return new ProtocolReadResult<P3DPacket?>();
    }
}