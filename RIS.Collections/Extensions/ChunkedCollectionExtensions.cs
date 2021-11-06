// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Runtime.CompilerServices;
using RIS.Collections.Chunked;

namespace RIS.Collections.Extensions
{
    public static class ChunkedCollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push<T>(
            this IChunkedCollection<T> collection, T value)
        {
            collection.Add(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pop<T>(
            this IChunkedCollection<T> collection)
        {
            var value = collection[collection.Length - 1L];

            collection.Remove();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Peek<T>(
            this IChunkedCollection<T> collection)
        {
            return collection[collection.Length - 1L];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T PeekRef<T>(
            this IChunkedArray<T> collection)
        {
            return ref collection
                .GetRef(collection.Length - 1L);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(
            this IChunkedCollection collection)
        {
            return collection.Length == 0;
        }
    }
}
