using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Extensions
{
    public static class ProcessExtensions
    {
        public static Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            process.EnableRaisingEvents = true;

            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>().CancelWith(cancellationToken);

            process.Exited += (sender, args) => taskCompletionSource.TrySetResult(((Process) sender).ExitCode);

            if (process.HasExited && !cancellationToken.IsCancellationRequested)
                return Task.FromResult(process.ExitCode);

            CancellationTokenRegistration tokenRegistration = cancellationToken.Register(state =>
                {
                    Process processState = (Process)state;

                    if (!processState.HasExited)
                        processState.Kill();
                },
                process);

            taskCompletionSource.Task.SynchronousContinueWith(
                (_, state) => ((CancellationTokenRegistration)state).Dispose(),
                tokenRegistration, TaskContinuationOptions.None, cancellationToken);

            return taskCompletionSource.Task;
        }
    }
}
