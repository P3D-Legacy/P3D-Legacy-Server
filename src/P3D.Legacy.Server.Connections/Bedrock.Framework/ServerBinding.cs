using Microsoft.AspNetCore.Connections;

using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework;

public abstract class ServerBinding
{
    public virtual ConnectionDelegate? Application { get; }

    public abstract IAsyncEnumerable<IConnectionListener> BindAsync(CancellationToken ct = default);
}