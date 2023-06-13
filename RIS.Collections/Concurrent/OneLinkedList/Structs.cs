// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Collections.Concurrent
{
    internal readonly struct ConcurrentOneLinkedListNodeThreadState<T>
    {
        public readonly int Phase;
        public readonly bool Pending;
        public readonly ConcurrentOneLinkedListNode<T> Node;

        public ConcurrentOneLinkedListNodeThreadState(int phase, bool pending, ConcurrentOneLinkedListNode<T> node)
        {
            Phase = phase;
            Pending = pending;
            Node = node;
        }
    }
}
