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
            Exception result = exception;

            while (result is AggregateException aggregateException)
            {
                if (aggregateException.InnerExceptions.Count == 0)
                    return result;

                result = aggregateException.InnerExceptions[0];
            }

            return result;
        }



        public static void ThrowFirst(this AggregateException exception)
        {
            throw UnwrapFirst(exception);
        }
    }
}
