﻿using P3D.Legacy.Common;
using P3D.Legacy.Server.Client.P3D.Data.P3DDatas;
using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Events;

namespace P3D.Legacy.Server.Client.P3D.Events;

public sealed record PlayerTradeOfferedP3DMonsterEvent(IPlayer Player, Origin Partner, TradeData Data) : IEvent;