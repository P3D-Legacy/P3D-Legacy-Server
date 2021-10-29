using MediatR;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public sealed record PlayerAuthenticateGameJoltCommand(IPlayer Player, GameJoltId GameJoltId) : IRequest<CommandResult> { }
}