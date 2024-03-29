﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Synchronization
{
    public sealed class OnceDisposer<T> : IDisposable
    {
        private readonly Action<T> _disposeFunc;
        private readonly T _state;
        private readonly OnceFlag _disposeFlag = new OnceFlag();

        public OnceDisposer(Action<T> disposeFunc, T state)
        {
            _disposeFunc = disposeFunc ?? throw new ArgumentNullException(nameof(disposeFunc));
            _state = state;
        }

        ~OnceDisposer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        private void Dispose(bool disposing)
        {
            if (!_disposeFlag.TrySet())
                return;

            if (disposing)
                GC.SuppressFinalize(this);

            _disposeFunc(_state);
        }
    }
}