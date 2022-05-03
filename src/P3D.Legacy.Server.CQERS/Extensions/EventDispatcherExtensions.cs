using P3D.Legacy.Server.CQERS.Events;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Extensions
{
    public static class EventDispatcherExtensions
    {
        public static Task DispatchAsync<TEvent>(this IEventDispatcher eventDispatcher, TEvent @event, CancellationToken ct) where TEvent : IEvent =>
            eventDispatcher.DispatchAsync(@event, eventDispatcher.DefaultStrategy, ct);
    }
}