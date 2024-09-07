using P3D.Legacy.Server.Domain.Events;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Domain.Extensions;

public static class EventDispatcherExtensions
{
    // TODO: We dropped cancellation token support for now, but we should add it back in.
    public static Task DispatchAsync<TEvent>(this IEventDispatcher eventDispatcher, TEvent @event, CancellationToken ct) where TEvent : IEvent =>
        eventDispatcher.DispatchAsync(@event, eventDispatcher.DefaultStrategy, trace: true, CancellationToken.None);
}