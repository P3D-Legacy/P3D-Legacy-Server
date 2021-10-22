﻿using Microsoft.AspNetCore.Connections.Features;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Chat;
using P3D.Legacy.Common.Packets.Server;
using P3D.Legacy.Common.Packets.Shared;
using P3D.Legacy.Server.Abstractions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public sealed partial class P3DConnectionContextHandler
    {
        private static GameDataPacket GetFromP3DPlayerState(IPlayer player, IP3DPlayerState state) => new()
        {
            Origin = player.Id,
            GameMode = state.GameMode,
            IsGameJoltPlayer = state.IsGameJoltPlayer,
            GameJoltId = player.GameJoltId,
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



        private static bool IsOfficialGameMode(string gamemode) =>
            string.Equals(gamemode, "Kolben", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(gamemode, "Pokemon3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(gamemode, "Pokémon3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(gamemode, "Pokemon 3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(gamemode, "Pokémon 3D", StringComparison.OrdinalIgnoreCase);

        public Task AssignIdAsync(long id, CancellationToken ct)
        {
            if (Id != 0)
                throw new InvalidOperationException("Id was already assigned!");

            Id = id;

            return Task.CompletedTask;
        }

        public Task AssignPermissionsAsync(PermissionFlags permissions, CancellationToken ct)
        {
            if (Permissions != PermissionFlags.None)
                throw new InvalidOperationException("Permissions were already assigned!");

            Permissions = permissions;

            return Task.CompletedTask;
        }

        public async Task KickAsync(string reason, CancellationToken ct)
        {
            await SendPacketAsync(new KickedPacket { Reason = reason }, ct);

            var lifetimeNotificationFeature = Features.Get<IConnectionLifetimeNotificationFeature>();
            lifetimeNotificationFeature.RequestClose();
        }

        private async Task SendPacketAsync(P3DPacket packet, CancellationToken ct)
        {
            using var span = _tracer.StartActiveSpan($"P3D Client Sending {packet.GetType().Name}");
            span.SetAttribute("p3dclient.packet_type", packet.GetType().FullName);
            await _writer.WriteAsync(_protocol, packet, ct);
        }

        private async Task SendServerMessageAsync(string text, CancellationToken ct) => await SendPacketAsync(new ChatMessageGlobalPacket
        {
            Origin = Origin.Server,
            Message = text
        }, ct);
    }
}