// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RIS.Collections.Concurrent
{
    public sealed class ConcurrentLinkedList<T> : IEnumerable<T>, IEnumerable where T : class
    {
        public static event EventHandler<RInformationEventArgs> Information;
		public static event EventHandler<RWarningEventArgs> Warning;
		public static event EventHandler<RErrorEventArgs> Error;

        internal readonly ConcurrentLinkedListNode<T> HeadNode;
        internal readonly ConcurrentLinkedListNode<T> TailNode;
        public ConcurrentLinkedListNode<T> Head
        {
            get
            {
                return HeadNode;
            }
        }
        public ConcurrentLinkedListNode<T> Tail
        {
            get
            {
                return TailNode;
            }
        }

        public ConcurrentLinkedList()
        {
            HeadNode = new ConcurrentLinkedListNode<T>(this);
            TailNode = new ConcurrentLinkedListNode<T>(this);

            HeadNode._prev = new ConcurrentLinkedListNodeLink<T>(null, false);
            HeadNode._next = new ConcurrentLinkedListNodeLinkPair<T>(null, new ConcurrentLinkedListNodeLink<T>(TailNode, false));
            TailNode._prev = new ConcurrentLinkedListNodeLink<T>(HeadNode, false);
            TailNode._next = new ConcurrentLinkedListNodeLinkPair<T>(null, new ConcurrentLinkedListNodeLink<T>(null, false));

            Thread.MemoryBarrier();
        }
        public ConcurrentLinkedList(IEnumerable<T> initial)
            : this()
        {
            if (initial == null)
                throw new ArgumentNullException(nameof(initial));

            foreach (T value in initial)
            {
                PushRight(value);
            }
        }

        public ConcurrentLinkedListNode<T> PushLeft(T value)
        {
            SpinWait spin = new SpinWait();
            ConcurrentLinkedListNode<T> node = new ConcurrentLinkedListNode<T>(this);
            ConcurrentLinkedListNode<T> prev = HeadNode;

            Thread.MemoryBarrier();
            ConcurrentLinkedListNode<T> next = prev._next.Link.Node;

            while (true)
            {
                node._prev = new ConcurrentLinkedListNodeLink<T>(prev, false);
                node._next = new ConcurrentLinkedListNodeLinkPair<T>(value, new ConcurrentLinkedListNodeLink<T>(next, false));

                bool b = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref prev._next,
                    new ConcurrentLinkedListNodeLink<T>(node, false),
                    new ConcurrentLinkedListNodeLink<T>(next, false));

                if (b)
                    break;

                next = prev._next.Link.Node;

                spin.SpinOnce();
            }

            PushEnd(node, next, spin);

            Thread.MemoryBarrier();

            return node;
        }

        public ConcurrentLinkedListNode<T> PushRight(T value)
        {
            SpinWait spin = new SpinWait();
            ConcurrentLinkedListNode<T> node = new ConcurrentLinkedListNode<T>(this);
            ConcurrentLinkedListNode<T> next = TailNode;

            Thread.MemoryBarrier();

            ConcurrentLinkedListNode<T> prev = next._prev.Node;

            while (true)
            {
                node._prev = new ConcurrentLinkedListNodeLink<T>(prev, false);
                node._next = new ConcurrentLinkedListNodeLinkPair<T>(value, new ConcurrentLinkedListNodeLink<T>(next, false));

                bool b = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref prev._next,
                    new ConcurrentLinkedListNodeLink<T>(node, false),
                    new ConcurrentLinkedListNodeLink<T>(next, false));

                if (b)
                    break;

                prev = CorrectPrev(prev, next);
                spin.SpinOnce();
            }

            PushEnd(node, next, spin);

            Thread.MemoryBarrier();

            return node;
        }

        private void PushEnd(ConcurrentLinkedListNode<T> node,
            ConcurrentLinkedListNode<T> next, SpinWait spin)
        {
            while (true)
            {
                Thread.MemoryBarrier();

                ConcurrentLinkedListNodeLink<T> link1 = next._prev;

                bool b = link1.D;

                if (!b)
                {
                    Thread.MemoryBarrier();

                    b |= !node._next.Link.Equals(new ConcurrentLinkedListNodeLink<T>(next, false));
                }

                if (b)
                    break;

                bool b1 = ConcurrentLinkedListHelper.CompareExchangeNodeLink(ref next._prev,
                    new ConcurrentLinkedListNodeLink<T>(node, false), link1);

                if (b1)
                {
                    bool b2 = node._prev.D;

                    if (b2)
                        CorrectPrev(node, next);

                    break;
                }

                spin.SpinOnce();
            }
        }

        public ConcurrentLinkedListNode<T> PopRight()
        {
            SpinWait spin = new SpinWait();
            ConcurrentLinkedListNode<T> next = TailNode;

            Thread.MemoryBarrier();

            ConcurrentLinkedListNode<T> node = next._prev.Node;

            while (true)
            {
                Thread.MemoryBarrier();

                bool b = !node._next.Link.Equals(new ConcurrentLinkedListNodeLink<T>(next, false));

                if (b)
                {
                    node = CorrectPrev(node, next);

                    continue;
                }

                if (node == HeadNode)
                {
                    Thread.MemoryBarrier();

                    return null;
                }

                bool b1 = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref node._next,
                    new ConcurrentLinkedListNodeLink<T>(next, true),
                    new ConcurrentLinkedListNodeLink<T>(next, false));

                if (b1)
                {
                    ConcurrentLinkedListNode<T> prev = node._prev.Node;

                    CorrectPrev(prev, next);

                    Thread.MemoryBarrier();

                    return node;
                }

                spin.SpinOnce();
            }
        }

        public void SetMark(ref ConcurrentLinkedListNodeLink<T> link)
        {
            Thread.MemoryBarrier();

            ConcurrentLinkedListNodeLink<T> node = link;

            while (!node.D)
            {
                ConcurrentLinkedListNodeLink<T> prevalent = Interlocked.CompareExchange(
                    ref link, new ConcurrentLinkedListNodeLink<T>(node.Node, true), node);

                if (prevalent == node)
                    break;

                node = prevalent;
            }
        }

        public ConcurrentLinkedListNode<T> CorrectPrev(ConcurrentLinkedListNode<T> prev,
            ConcurrentLinkedListNode<T> node)
        {
            return CorrectPrev(prev, node, new SpinWait());
        }

        public ConcurrentLinkedListNode<T> CorrectPrev(ConcurrentLinkedListNode<T> prev,
            ConcurrentLinkedListNode<T> node, SpinWait spin)
        {
            ConcurrentLinkedListNode<T> lastLink = null;
            while (true)
            {

                Thread.MemoryBarrier();

                ConcurrentLinkedListNodeLink<T> link1 = node._prev;

                if (link1.D)
                    break;

                Thread.MemoryBarrier();

                ConcurrentLinkedListNodeLink<T> prev2 = prev._next.Link;

                if (prev2.D)
                {
                    if (lastLink != null)
                    {
                        SetMark(ref prev._prev);

                        _ = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref lastLink._next,
                            (ConcurrentLinkedListNodeLink<T>)prev2.Node, (ConcurrentLinkedListNodeLink<T>)prev);

                        prev = lastLink;
                        lastLink = null;

                        continue;
                    }

                    Thread.MemoryBarrier();

                    prev2 = prev._prev;
                    prev = prev2.Node;

                    continue;
                }

                if (prev2.Node != node)
                {
                    lastLink = prev;
                    prev = prev2.Node;

                    continue;
                }

                bool b = ConcurrentLinkedListHelper.CompareExchangeNodeLink(ref node._prev,
                    new ConcurrentLinkedListNodeLink<T>(prev, false), link1);

                if (b)
                {
                    bool b1 = prev._prev.D;

                    if (b1)
                        continue;

                    break;
                }

                spin.SpinOnce();
            }

            return prev;
        }

        public IEnumerator<T> GetEnumerator()
        {
            ConcurrentLinkedListNode<T> current = Head;

            while (true)
            {
                current = current.Next;

                if (current == Tail)
                    yield break;

                yield return current.Value;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
