// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static Task AsTask(this CancellationToken cancellationToken)
        {
            return new TaskCompletionSource<bool>()
                .CancelWith(cancellationToken)
                .Task;
        }
    }
}
