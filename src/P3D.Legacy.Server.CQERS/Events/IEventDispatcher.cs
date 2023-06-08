using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Events
{
    /// <summary>
    /// Strategy to use when publishing Events
    /// </summary>
    public enum DispatchStrategy
    {
        /// <summary>
        /// Run each Event handler after one another. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
        /// </summary>
        SyncContinueOnException = 0,

        /// <summary>
        /// Run each Event handler after one another. Returns when all handlers are finished or an exception has been thrown. In case of an exception, any handlers after that will not be run.
        /// </summary>
        SyncStopOnException = 1,

        /// <summary>
        /// Run all Event handlers asynchronously. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
        /// </summary>
        Async = 2,

        /// <summary>
        /// Run each Event handler on it's own thread using Task.Run(). Returns immediately and does not wait for any handlers to finish. Note that you cannot capture any exceptions, even if you await the call to Publish.
        /// </summary>
        ParallelNoWait = 3,

        /// <summary>
        /// Run each Event handler on it's own thread using Task.Run(). Returns when all threads (handlers) are finished. In case of any exception(s), they are captured in an AggregateException by Task.WhenAll.
        /// </summary>
        ParallelWhenAll = 4,

        /// <summary>
        /// Run each Event handler on it's own thread using Task.Run(). Returns when any thread (handler) is finished. Note that you cannot capture any exceptions (See msdn documentation of Task.WhenAny)
        /// </summary>
        ParallelWhenAny = 5,
    }

    public interface IEventDispatcher
    {
        DispatchStrategy DefaultStrategy { get; set; }

        Task DispatchAsync<TEvent>(TEvent @event, DispatchStrategy dispatchStrategy, bool trace, CancellationToken ct) where TEvent : IEvent;
    }
}