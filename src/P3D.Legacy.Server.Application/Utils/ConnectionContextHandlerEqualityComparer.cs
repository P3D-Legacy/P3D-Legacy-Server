using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Application.Utils
{
    public sealed class ConnectionContextHandlerEqualityComparer : IEqualityComparer<ConnectionContextHandler>
    {
        public bool Equals(ConnectionContextHandler? x, ConnectionContextHandler? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.ConnectionId, y.ConnectionId, StringComparison.Ordinal);
        }

        public int GetHashCode(ConnectionContextHandler obj) => HashCode.Combine(obj.ConnectionId);
    }
}