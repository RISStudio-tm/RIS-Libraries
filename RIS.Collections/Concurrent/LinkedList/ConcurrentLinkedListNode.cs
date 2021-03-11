// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;

namespace RIS.Collections.Concurrent
{
    public class ConcurrentLinkedListNode<T>
        where T : class
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        internal ConcurrentLinkedListNodeLinkPair<T> _next;
        public ConcurrentLinkedListNode<T> Next
        {
            get
            {
                ConcurrentLinkedListNode<T> cursor = this;
                bool b = ToNext(ref cursor);

                Thread.MemoryBarrier();

                if (b)
                    return cursor;

                return null;
            }
        }
        internal ConcurrentLinkedListNodeLink<T> _prev;
        public ConcurrentLinkedListNode<T> Prev
        {
            get
            {
                ConcurrentLinkedListNode<T> cursor = this;
                bool b = ToPrev(ref cursor);

                Thread.MemoryBarrier();

                if (b)
                    return cursor;

                return null;
            }
        }
        public ConcurrentLinkedList<T> List { get; }
        public bool IsDummyNode
        {
            get
            {
                return this == List.HeadNode || this == List.TailNode;
            }
        }
        public T Value
        {
            get
            {
                if (IsDummyNode)
                {
                    var exception = new InvalidOperationException(
                            "The current node is the dummy head or dummy tail node of the current List, so it may not store any value.");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                Thread.MemoryBarrier();

                T value = _next.Value;

                Thread.MemoryBarrier();

                return value;
            }
            set
            {
                if (IsDummyNode)
                {
                    var exception = new InvalidOperationException(
                        "The current node is the dummy head or dummy tail node of the current List, so it may not store any value.");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                Thread.MemoryBarrier();

                while (true)
                {
                    ConcurrentLinkedListNodeLinkPair<T> currentPair = _next;

                    if (Interlocked.CompareExchange(
                        ref _next,
                        new ConcurrentLinkedListNodeLinkPair<T>(value, currentPair.Link),
                        currentPair) == currentPair)
                    {
                        break;
                    }
                }
            }
        }
        public bool Removed
        {
            get
            {
                Thread.MemoryBarrier();
                bool result = _next.Link.D;
                Thread.MemoryBarrier();
                return result;
            }
        }

        public ConcurrentLinkedListNode(ConcurrentLinkedList<T> list)
        {
            List = list;
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public ConcurrentLinkedListNode<T> InsertBefore(T newValue)
        {
            ConcurrentLinkedListNode<T> result = InsertBefore(newValue, this);
            Thread.MemoryBarrier();
            return result;
        }
        private ConcurrentLinkedListNode<T> InsertBefore(
            T value, ConcurrentLinkedListNode<T> cursor, SpinWait spin = new SpinWait())
        {
            if (cursor == List.HeadNode)
                return InsertAfter(value);
            ConcurrentLinkedListNode<T> node = new ConcurrentLinkedListNode<T>(List);

            Thread.MemoryBarrier();
            ConcurrentLinkedListNode<T> prev = cursor._prev.Node;

            ConcurrentLinkedListNode<T> next;
            while (true)
            {
                while (true)
                {
                    Thread.MemoryBarrier();

                    bool b = !cursor._next.Link.D;

                    if (b)
                        break;

                    ToNext(ref cursor);

                    Thread.MemoryBarrier();

                    prev = cursor._prev.Node;
                    prev = List.CorrectPrev(prev, cursor);
                }

                next = cursor;
                node._prev = new ConcurrentLinkedListNodeLink<T>(prev, false);
                node._next = new ConcurrentLinkedListNodeLinkPair<T>(value,
                    new ConcurrentLinkedListNodeLink<T>(next, false));

                bool b1 = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref prev._next,
                    new ConcurrentLinkedListNodeLink<T>(node, false),
                    new ConcurrentLinkedListNodeLink<T>(cursor, false));

                if (b1)
                    break;

                prev = List.CorrectPrev(prev, cursor);
                spin.SpinOnce();
            }

            List.CorrectPrev(prev, next);

            return node;
        }

        public ConcurrentLinkedListNode<T> InsertAfter(T newValue)
        {
            ConcurrentLinkedListNode<T> result = InsertAfter(newValue, this);
            Thread.MemoryBarrier();
            return result;
        }
        private ConcurrentLinkedListNode<T> InsertAfter(
            T value, ConcurrentLinkedListNode<T> cursor, SpinWait spin = new SpinWait())
        {
            if (cursor == List.TailNode)
                return InsertBefore(value, cursor, spin);

            ConcurrentLinkedListNode<T> node = new ConcurrentLinkedListNode<T>(List);
            ConcurrentLinkedListNode<T> prev = cursor;
            ConcurrentLinkedListNode<T> next;

            while (true)
            {
                Thread.MemoryBarrier();

                next = prev._next.Link.Node;
                node._prev = new ConcurrentLinkedListNodeLink<T>(prev, false);
                node._next = new ConcurrentLinkedListNodeLinkPair<T>(value,
                    new ConcurrentLinkedListNodeLink<T>(next, false));

                bool b1 = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref cursor._next,
                    new ConcurrentLinkedListNodeLink<T>(node, false),
                    new ConcurrentLinkedListNodeLink<T>(next, false));

                if (b1)
                    break;

                bool b = prev._next.Link.D;

                if (b)
                    return InsertBefore(value, cursor, spin);

                spin.SpinOnce();
            }

            List.CorrectPrev(prev, next);

            return node;
        }

        public ConcurrentLinkedListNode<T> InsertAfterIf(T newValue, Func<T, bool> condition)
        {
            if (IsDummyNode)
                return null;

            SpinWait spin = new SpinWait();
            ConcurrentLinkedListNode<T> cursor = this;
            ConcurrentLinkedListNode<T> node = new ConcurrentLinkedListNode<T>(List);
            ConcurrentLinkedListNode<T> prev = cursor;
            ConcurrentLinkedListNode<T> next;
            while (true)
            {

                Thread.MemoryBarrier();
                ConcurrentLinkedListNodeLinkPair<T> nextLink = prev._next;

                next = nextLink.Link.Node;
                node._prev = new ConcurrentLinkedListNodeLink<T>(prev, false);
                node._next = new ConcurrentLinkedListNodeLinkPair<T>(newValue,
                    new ConcurrentLinkedListNodeLink<T>(next, false));

                bool cexSuccess;
                ConcurrentLinkedListNodeLinkPair<T> currentPair = nextLink;
                while (true)
                {
                    if (!condition(currentPair.Value))
                    {
                        Thread.MemoryBarrier();
                        return null;
                    }

                    if (!currentPair.Link.Equals(
                        new ConcurrentLinkedListNodeLink<T>(next, false)))
                    {
                        cexSuccess = false;
                        break;
                    }

                    ConcurrentLinkedListNodeLinkPair<T> prevalent
                        = Interlocked.CompareExchange
                        (ref cursor._next,
                            new ConcurrentLinkedListNodeLinkPair<T>(currentPair.Value,
                                new ConcurrentLinkedListNodeLink<T>(node, false)),
                            currentPair);

                    if (ReferenceEquals(prevalent, currentPair))
                    {
                        cexSuccess = true;
                        break;
                    }

                    currentPair = prevalent;
                }

                if (cexSuccess)
                    break;

                if (currentPair.Link.D)
                {
                    Thread.MemoryBarrier();
                    return null;
                }

                spin.SpinOnce();
            }

            List.CorrectPrev(prev, next);
            Thread.MemoryBarrier();
            return node;
        }

        private bool ToNext(ref ConcurrentLinkedListNode<T> cursor)
        {
            while (true)
            {
                if (cursor == List.TailNode)
                    return false;

                Thread.MemoryBarrier();

                ConcurrentLinkedListNode<T> next = cursor._next.Link.Node;

                Thread.MemoryBarrier();

                bool d = next._next.Link.D;

                bool b = d;
                if (b)
                {
                    Thread.MemoryBarrier();

                    b &= !cursor._next.Link.Equals(new ConcurrentLinkedListNodeLink<T>(next, true));
                }

                if (b)
                {
                    List.SetMark(ref next._prev);

                    Thread.MemoryBarrier();

                    ConcurrentLinkedListNode<T> p = next._next.Link.Node;

                    _ = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref cursor._next,
                        (ConcurrentLinkedListNodeLink<T>)p, (ConcurrentLinkedListNodeLink<T>)next);

                    continue;
                }

                cursor = next;

                if (!d)
                    return true;
            }
        }

        private bool ToPrev(ref ConcurrentLinkedListNode<T> cursor)
        {
            while (true)
            {
                if (cursor == List.HeadNode)
                    return false;

                Thread.MemoryBarrier();

                ConcurrentLinkedListNode<T> prev = cursor._prev.Node;

                Thread.MemoryBarrier();

                bool b = prev._next.Link.Equals(new ConcurrentLinkedListNodeLink<T>(cursor, false));

                if (b)
                {
                    Thread.MemoryBarrier();
                    b &= !cursor._next.Link.D;
                }

                if (b)
                {
                    cursor = prev;

                    return true;
                }
                else
                {
                    Thread.MemoryBarrier();

                    bool b1 = cursor._next.Link.D;

                    if (b1)
                        ToNext(ref cursor);
                    else
                        List.CorrectPrev(prev, cursor);
                }
            }
        }

        public bool Remove() // out T lastValue
        {
            if (IsDummyNode)
            {
                // lastValue = default(T);

                return false;
            }

            while (true)
            {
                Thread.MemoryBarrier();

                ConcurrentLinkedListNodeLink<T> next = _next.Link;

                if (next.D)
                {
                    // lastValue = default(T);

                    Thread.MemoryBarrier();

                    return false;
                }

                bool b = ConcurrentLinkedListHelper.CompareExchangeNodeLinkPair(ref _next,
                    new ConcurrentLinkedListNodeLink<T>(next.Node, true), next);

                if (b)
                {
                    ConcurrentLinkedListNodeLink<T> prev;

                    while (true)
                    {
                        prev = _prev;

                        if (prev.D)
                            break;

                        bool b1 = ConcurrentLinkedListHelper.CompareExchangeNodeLink(ref _prev,
                            new ConcurrentLinkedListNodeLink<T>(prev.Node, true), prev);

                        if (b1)
                            break;
                    }

                    List.CorrectPrev(prev.Node, next.Node);
                    //lastValue = this.newValue;

                    Thread.MemoryBarrier();

                    return true;
                }
            }
        }

        public T CompareExchangeValue(T newValue, T comparand)
        {
            ConcurrentLinkedListNodeLinkPair<T> currentPair;

            Thread.MemoryBarrier();

            while (true)
            {
                currentPair = _next;

                if (!ReferenceEquals(currentPair.Value, comparand))
                    return currentPair.Value;

                if (ReferenceEquals(
                    Interlocked.CompareExchange(
                        ref _next,
                        new ConcurrentLinkedListNodeLinkPair<T>(
                            newValue, currentPair.Link),
                        currentPair),
                    currentPair))
                {
                    break;
                }
            }

            return currentPair.Value;
        }
    }
}
