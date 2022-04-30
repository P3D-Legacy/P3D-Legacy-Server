using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;

namespace P3D.Legacy.Server.Application.Commands.Administration
{
    public sealed record KickPlayerCommand(IPlayer Player, string Reason) : ICommand;
}