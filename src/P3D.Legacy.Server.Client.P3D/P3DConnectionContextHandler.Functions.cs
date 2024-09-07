using Microsoft.AspNetCore.Connections.Features;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Client.P3D.Packets;
using P3D.Legacy.Server.Client.P3D.Packets.Chat;
using P3D.Legacy.Server.Client.P3D.Packets.Client;
using P3D.Legacy.Server.Client.P3D.Packets.Common;
using P3D.Legacy.Server.Client.P3D.Packets.Server;
using P3D.Legacy.Server.Client.P3D.Services;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D;

// ReSharper disable once ArrangeTypeModifiers
internal partial class P3DConnectionContextHandler
{
    private static bool IsOfficialGameMode(string gamemode) =>
        string.Equals(gamemode, "Kolben", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(gamemode, "Pokemon3D", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(gamemode, "Pokémon3D", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(gamemode, "Pokemon 3D", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(gamemode, "Pokémon 3D", StringComparison.OrdinalIgnoreCase);

    private static GameDataPacket GetFromP3DPlayerState(IPlayer player, IP3DPlayerState state) => new()
    {
        Origin = player.Origin,
        GameMode = state.GameMode,
        IsGameJoltPlayer = state.IsGameJoltPlayer,
        GameJoltId = player.Id.GameJoltIdOrNone.Value,
        DecimalSeparator = state.DecimalSeparator,
        Name = player.Name,
        LevelFile = state.LevelFile,
        Position = state.Position,
        Facing = state.Facing,
        Moving = state.Moving,
        Skin = state.Skin,
        BusyType = state.BusyType,
        MonsterVisible = state.MonsterVisible,
        MonsterPosition = state.MonsterPosition,
        MonsterSkin = state.MonsterSkin,
        MonsterFacing = state.MonsterFacing
    };

    public Task<GameJoltId> GetGameJoltIdOrNoneAsync(CancellationToken ct) => Task.FromResult(GameJoltId);

    public Task AssignIdAsync(PlayerId id, CancellationToken ct)
    {
        Debug.Assert(State == PlayerState.Initializing);

        if (!Id.IsNone)
            throw new InvalidOperationException("Id was already assigned!");

        Id = id;

        return Task.CompletedTask;
    }

    public Task AssignOriginAsync(Origin origin, CancellationToken ct)
    {
        Debug.Assert(State == PlayerState.Initializing);

        if (Origin.IsPlayer)
            throw new InvalidOperationException("Origin was already assigned!");

        Origin = origin;

        return Task.CompletedTask;
    }

    public Task AssignPermissionsAsync(PermissionTypes permissions, CancellationToken ct)
    {
        Debug.Assert(State == PlayerState.Authentication);

        Permissions = permissions;

        return Task.CompletedTask;
    }

    public async Task KickAsync(string reason, CancellationToken ct)
    {
        Debug.Assert(State is PlayerState.Initializing or PlayerState.Authentication or PlayerState.Initialized);

        await SendPacketAsync(new KickedPacket { Reason = reason }, ct);

        var lifetimeNotificationFeature = Connection.Features.Get<IConnectionLifetimeNotificationFeature>();
        lifetimeNotificationFeature?.RequestClose();
    }

    private async Task SendPacketAsync(P3DPacket packet, CancellationToken ct)
    {
        Debug.Assert(State is not PlayerState.Finalizing and not PlayerState.Finalized);

        if (State == PlayerState.Finalized) return;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, Connection.ConnectionClosed);

        try
        {
            using var span = _tracer.StartActiveSpan($"P3D Client Sending {packet.GetType().Name}", SpanKind.Client);
            span.SetAttribute("client.address", IPEndPoint.Address.ToString());
            span.SetAttribute("client.port", IPEndPoint.Port);
            span.SetAttribute("network.transport", "tcp");
            span.SetAttribute("network.protocol.name", "p3d");
            span.SetAttribute("network.protocol.version", packet.Protocol.ToString());
            span.SetAttribute("enduser.id", Name);
            span.SetAttribute("enduser.role", Permissions.ToString());
            span.SetAttribute("p3dclient.packet_type", packet.GetType().FullName);
            span.SetAttribute("peer.service", $"{Name} (P3D-Legacy)");

            // Do not trace the ping packet
            if (packet is PingPacket && Activity.Current is not null)
                Activity.Current.IsAllDataRequested = false;
            // Disable tracing if it's from non trackable services. TODO: Remove direct service dependance
            if (_movementCompensationService.IsFromService.Value && Activity.Current is not null)
                Activity.Current.IsAllDataRequested = false;

            await _writer.WriteAsync(_protocol, packet, cts.Token);
        }
        // Catch Writing is not allowed after writer was completed.
        // We can't catch it in worst case scenarios
        catch (InvalidOperationException) { }
        catch (OperationCanceledException) when (cts.IsCancellationRequested) { }
    }

    private async Task SendServerMessageAsync(string text, CancellationToken ct) => await SendPacketAsync(new ChatMessageGlobalPacket
    {
        Origin = Origin.Server,
        Message = text,
    }, ct);
}