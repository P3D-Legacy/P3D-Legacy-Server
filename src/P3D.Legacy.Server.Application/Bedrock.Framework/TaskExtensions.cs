using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework;

internal static class TaskExtensions
{
    public static async Task<bool> WithCancellationAsync(this Task task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        // This disposes the registration as soon as one of the tasks trigger
        await using var register = cancellationToken.Register(state =>
        {
            if (state is TaskCompletionSource<object?> stateTcs)
            {
                stateTcs.TrySetResult(null);
            }
        }, tcs);

        var resultTask = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
        if (resultTask == tcs.Task)
        {
            // Operation cancelled
            return false;
        }

        await task.ConfigureAwait(false);
        return true;
    }

    public static async Task<bool> TimeoutAfterAsync(this Task task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var delayTask = Task.Delay(timeout, cts.Token);

        var resultTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
        if (resultTask == delayTask)
        {
            // Operation cancelled
            return false;
        }

        // Cancel the timer task so that it does not fire
        await cts.CancelAsync();

        await task.ConfigureAwait(false);
        return true;
    }
}