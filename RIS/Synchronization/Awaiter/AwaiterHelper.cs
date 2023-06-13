// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Synchronization
{
    public static class AwaiterHelper
    {
        /// <summary>
        ///     Код <code><see langword="await"/> <see cref="AwaiterHelper"/>.DetachSynchronizationContext();</code> гарантирует, что после его вызова выполнение кода, следующего за ним, продолжится без контекста синхронизации.
        /// </summary>
        public static DetachSynchronizationContextAwaiter DetachSynchronizationContext()
        {
            return new DetachSynchronizationContextAwaiter();
        }
    }
}
