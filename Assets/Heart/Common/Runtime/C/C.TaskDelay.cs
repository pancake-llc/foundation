using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pancake.Common
{
    public static partial class C
    {
        /// <summary>
        /// Delay using async/await
        /// </summary>
        /// <param name="delay">delay time(seconds)</param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void TaskDelayInvoke(float delay, Action action, CancellationToken cancellationToken = default)
        {
            // Do not remove the casting; otherwise, it will call to itself, creating an infinite loop
            TaskDelayInvoke(C.SecToMs(delay), action, cancellationToken);
        }

        public static async Task TaskDelaySeconds(float seconds, CancellationToken cancellationToken = default)
        {
            await TaskDelay(C.SecToMs(seconds), cancellationToken);
        }

        public static async Task TaskDelay(int milliseconds, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(milliseconds, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // This exception is thrown by the task cancellation design, not an actual error.
            }
        }

        public static bool IsCanceled(this CancellationTokenSource source) { return source == null || source.IsCancellationRequested; }

        private static async void TaskDelayInvoke(int milliseconds, Action action, CancellationToken cancellationToken = default)
        {
            if (milliseconds <= 0) return;

            try
            {
                await Task.Delay(milliseconds, cancellationToken);
                action.Invoke();
            }
            catch (TaskCanceledException)
            {
                // This exception is thrown by the task cancellation design, not an actual error.
            }
        }
    }
}