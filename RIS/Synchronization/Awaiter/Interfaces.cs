// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace RIS.Synchronization
{
    public interface IAwaiterCore : ICriticalNotifyCompletion
    {
        bool IsCompleted { get; }
    }



    public interface IAwaiter : IAwaiterCore
    {
        void GetResult();
    }

    public interface IAwaiter<out T> : IAwaiterCore
    {
        T GetResult();
    }



    public interface IAwaitable<out TAwaiter>
        where TAwaiter : IAwaiter
    {
        TAwaiter GetAwaiter();
    }

    public interface IAwaitable<TResult, out TAwaiter>
        where TAwaiter : IAwaiter<TResult>
    {
        TAwaiter GetAwaiter();
    }
}
