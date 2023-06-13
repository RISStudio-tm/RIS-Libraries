// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Collections.Concurrent
{
    public class ConcurrentLinkedListNodeLink<T>
        where T : class
    {
        public ConcurrentLinkedListNode<T> Node { get; }
        public bool D { get; }

        public ConcurrentLinkedListNodeLink(ConcurrentLinkedListNode<T> p, bool d)
        {
            Node = p;
            D = d;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConcurrentLinkedListNodeLink<T>))
                return false;

            var other = (ConcurrentLinkedListNodeLink<T>)obj;

            return D == other.D && Node == other.Node;
        }
        public bool Equals(ConcurrentLinkedListNodeLink<T> other)
        {
            if (other == null)
                return false;

            return D == other.D && Node == other.Node;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 486187739 + Node.GetHashCode();
            hash = hash * 486187739 + D.GetHashCode();

            return hash;
        }

        public static explicit operator ConcurrentLinkedListNodeLink<T>(ConcurrentLinkedListNode<T> node)
        {
            return new ConcurrentLinkedListNodeLink<T>(node, false);
        }
        public static explicit operator ConcurrentLinkedListNode<T>(ConcurrentLinkedListNodeLink<T> link)
        {
            return link.Node;
        }
    }
}
