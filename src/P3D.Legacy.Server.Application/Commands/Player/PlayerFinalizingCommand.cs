﻿using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.CQERS.Commands;

namespace P3D.Legacy.Server.Application.Commands.Player
{
    public sealed record PlayerFinalizingCommand(IPlayer Player) : ICommand;
}