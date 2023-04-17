// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class AsyncDisposable : IAsyncDisposable
    {
        private Func<ValueTask> _disposeFunction;



        private AsyncDisposable(
            Func<ValueTask> disposeFunction)
        {
            _disposeFunction = disposeFunction;
        }



        public ValueTask DisposeAsync()
        {
            return Interlocked
                .Exchange(ref _disposeFunction, null)?
                .Invoke() ?? default;
        }



        public static AsyncDisposable Create(
            Func<ValueTask> disposeFunction)
        {
            return new AsyncDisposable(
                disposeFunction);
        }
    }
}