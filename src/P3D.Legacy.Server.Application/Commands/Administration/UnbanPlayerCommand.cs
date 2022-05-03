using P3D.Legacy.Common;
using P3D.Legacy.Server.CQERS.Commands;

namespace P3D.Legacy.Server.Application.Commands.Administration
{
    public sealed record UnbanPlayerCommand(PlayerId Id) : ICommand;
}