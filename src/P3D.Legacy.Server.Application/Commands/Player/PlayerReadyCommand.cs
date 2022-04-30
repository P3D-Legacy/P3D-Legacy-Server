using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public sealed record PlayerReadyCommand(IPlayer Player) : ICommand;
}