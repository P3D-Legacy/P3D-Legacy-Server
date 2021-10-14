﻿using MediatR;

using P3D.Legacy.Server.Abstractions;

namespace P3D.Legacy.Server.Notifications
{
    public sealed record PlayerJoinedNotification(IPlayer Player) : INotification;
}