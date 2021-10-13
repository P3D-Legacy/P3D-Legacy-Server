using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Common.Packets.Chat;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services.Server
{
    public partial class P3DConnectionContextHandler
    {
        private bool IsOfficialGameMode =>
            string.Equals(GameMode, "Kolben", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokemon3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokémon3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokemon 3D", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(GameMode, "Pokémon 3D", StringComparison.OrdinalIgnoreCase);

        public Task AssignIdAsync(long id, CancellationToken ct)
        {
            if (Id != 0)
                throw new InvalidOperationException("Id was already assigned!");

            Id = id;

            return Task.CompletedTask;
        }

        private async Task SendPacketAsync(P3DPacket packet, CancellationToken ct) => await _writer.WriteAsync(_protocol, packet, ct);

        private async Task SendServerMessageAsync(string text, CancellationToken ct) => await SendPacketAsync(new ChatMessageGlobalPacket
        {
            Origin = Origin.Server,
            Message = text
        }, ct);
    }
}
