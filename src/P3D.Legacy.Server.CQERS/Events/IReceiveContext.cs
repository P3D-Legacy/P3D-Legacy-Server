using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Events
{
    public interface IReceiveContext<out TEvent> where TEvent : IEvent
    {
        TEvent Message { get; }

        Task PublishAsync(IEvent @event, DispatchStrategy strategy, CancellationToken ct = default);
    }
}