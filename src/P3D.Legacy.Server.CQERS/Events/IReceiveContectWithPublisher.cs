using P3D.Legacy.Server.Domain.Events;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Events;

public interface IReceiveContectWithPublisher<out TEvent> : IReceiveContext<TEvent> where TEvent : IEvent
{
    Task PublishAsync(IEvent @event, DispatchStrategy strategy, CancellationToken ct = default);
}