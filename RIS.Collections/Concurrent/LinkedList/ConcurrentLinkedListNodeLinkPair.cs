// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Collections.Concurrent
{
    public class ConcurrentLinkedListNodeLinkPair<T>
        where T : class
    {
        public T Value { get; }
        public ConcurrentLinkedListNodeLink<T> Link { get; }

        public ConcurrentLinkedListNodeLinkPair(T value, ConcurrentLinkedListNodeLink<T> link)
        {
            Value = value;
            Link = link;
        }
    }
}
