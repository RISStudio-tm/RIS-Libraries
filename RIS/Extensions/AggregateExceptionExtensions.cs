// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Extensions
{
    public static class AggregateExceptionExtensions
    {
        public static Exception UnwrapFirst(this AggregateException exception)
        {
            return UnwrapFirstInternal(exception);
        }
        private static Exception UnwrapFirstInternal(this AggregateException exception)
        {
            if (exception.InnerExceptions.Count == 1)
                return exception.InnerExceptions[0];

            return UnwrapFirstInternal(exception);
        }

        public static void ThrowUnwrapFirst(this AggregateException exception)
        {
            throw UnwrapFirst(exception);
        }
    }
}
