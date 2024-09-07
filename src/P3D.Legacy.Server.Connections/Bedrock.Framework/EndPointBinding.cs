using Microsoft.AspNetCore.Connections;

using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework;

public class EndPointBinding(EndPoint endPoint, ConnectionDelegate application, IConnectionListenerFactory connectionListenerFactory) : ServerBinding
{
    public override ConnectionDelegate Application { get; } = application;

    public override async IAsyncEnumerable<IConnectionListener> BindAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        yield return await connectionListenerFactory.BindAsync(endPoint, ct);
    }

    public override string? ToString() => endPoint.ToString();
}