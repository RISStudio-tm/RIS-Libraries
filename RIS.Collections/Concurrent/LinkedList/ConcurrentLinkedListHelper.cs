// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;

namespace RIS.Collections.Concurrent
{
    public static class ConcurrentLinkedListHelper
    {
        public static (T, bool) CompareExchangeValueIf<T>(this ConcurrentLinkedListNode<T> node, Func<T, T> newValue)
            where T : class
        {
            T old = node.Value;

            while (true)
            {
                T @new = newValue(old);

                if (@new == null)
                    return (old, false);

                T prevalent = node.CompareExchangeValue(@new, old);

                if (prevalent == old)
                    return (old, true);

                old = prevalent;
            }
        }

        public static bool ConditionalCompareExchange<T>(ref T location, T value, Func<T, bool> condition, out T current)
            where T : class
        {
            Thread.MemoryBarrier();

            current = location;

            while (true)
            {
                if (!condition(current))
                    return false;

                T prevalent = Interlocked.CompareExchange(ref location, value, current);

                if (ReferenceEquals(prevalent, current))
                    return true;

                current = prevalent;
            }
        }
        public static bool ConditionalCompareExchange<T>(ref T location, T value, Func<T, bool> condition)
            where T : class
        {
            return ConditionalCompareExchange(ref location, value, condition, out _);
        }

        public static bool CompareExchangeNodeLink<T>(ref ConcurrentLinkedListNodeLink<T> location,
            ConcurrentLinkedListNodeLink<T> value, ConcurrentLinkedListNodeLink<T> comparandByValue)
            where T : class
        {
            return ConditionalCompareExchange(ref location, value,
                original => original.Equals(comparandByValue));
        }

        public static bool CompareExchangeNodeLinkPair<T>(ref ConcurrentLinkedListNodeLinkPair<T> location,
            ConcurrentLinkedListNodeLink<T> newLink, ConcurrentLinkedListNodeLink<T> comparandByValue)
            where T : class
        {
            Thread.MemoryBarrier();

            T currentValue = location.Value;

            return ConditionalCompareExchange(ref location,
                new ConcurrentLinkedListNodeLinkPair<T>(currentValue, newLink),
                original => original.Link.Equals(comparandByValue) && ReferenceEquals(original.Value, currentValue));
        }
    }
}
