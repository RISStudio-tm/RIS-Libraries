// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Exceptions
{
    public static class ExceptionsUtils
    {
        public static Exception UnwrapFirst(AggregateException exception)
        {
            return UnwrapFirstInternal(exception);
        }
        internal static Exception UnwrapFirstInternal(AggregateException exception)
        {
            if (exception.InnerExceptions.Count == 1)
                return exception.InnerExceptions[0];

            return UnwrapFirstInternal(exception);
        }

        public static void ThrowUnwrapFirst(AggregateException exception)
        {
            throw UnwrapFirst(exception);
        }
    }
}
