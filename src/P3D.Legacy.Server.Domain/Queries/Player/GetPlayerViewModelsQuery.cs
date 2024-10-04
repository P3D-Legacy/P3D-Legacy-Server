﻿using System.Collections.Immutable;

namespace P3D.Legacy.Server.Domain.Queries.Player;

public sealed record GetPlayerViewModelsQuery : IQuery<ImmutableArray<PlayerViewModel>>;