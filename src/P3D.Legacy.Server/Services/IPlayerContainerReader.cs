﻿using P3D.Legacy.Server.Models;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public interface IPlayerContainerReader
    {
        Task<IPlayer?> GetAsync(ulong id, CancellationToken ct);
        IAsyncEnumerable<IPlayer> GetAllAsync(CancellationToken ct);
    }
}