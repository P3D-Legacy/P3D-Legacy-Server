﻿using P3D.Legacy.Common;
using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events;

public sealed record PlayerTradeAcceptedEvent(IPlayer Target, Origin Initiator) : IEvent;