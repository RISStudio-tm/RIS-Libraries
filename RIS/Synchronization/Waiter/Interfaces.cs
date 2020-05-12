using System;
using System.Runtime.CompilerServices;

namespace RIS.Synchronization
{
    public interface IAwaiter<out TResult> : ICriticalNotifyCompletion
    {
        bool IsCompleted { get; }

        TResult GetResult();
    }

    public interface IAwaitable<out TResult>
    {
        IAwaiter<TResult> GetAwaiter();
    }
}
