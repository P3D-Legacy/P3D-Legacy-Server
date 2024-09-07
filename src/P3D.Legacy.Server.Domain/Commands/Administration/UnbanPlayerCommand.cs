using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Domain.Commands.Administration;

public sealed record UnbanPlayerCommand(PlayerId Id) : ICommand;