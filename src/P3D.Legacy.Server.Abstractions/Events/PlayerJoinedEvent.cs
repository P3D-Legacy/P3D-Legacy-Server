﻿using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events
{
    public sealed record PlayerJoinedEvent(IPlayer Player) : IEvent;
}