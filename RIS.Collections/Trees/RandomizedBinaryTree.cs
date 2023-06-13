// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using RIS.Randomizing;

namespace RIS.Collections.Trees
{
    public sealed class RandomizedBinaryTree<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        private sealed class Node
        {
            internal Node Left;
            internal Node Right;
            internal int Count;
            internal TKey Key;
            internal TValue Value;

            public override string ToString()
            {
                return $"({Key}, {Value}): Count = {Count}";
            }

            public static int ComputeCount(Node left, Node right)
            {
                return (left?.Count ?? 0) + (right?.Count ?? 0) + 1;
            }
        }

        private Node _root;
        private readonly Random _random = Rand.CreateRandom();

        public IComparer<TKey> Comparer { get; }
        public int Count
        {
            get
            {
                return _root?.Count ?? 0;
            }
        }
        public KeyValuePair<TKey, TValue> Min
        {
            get
            {
                if (_root == null)
                {
                    throw new InvalidOperationException("the collection is empty");
                }

                Node node = _root;

                while (node.Left != null)
                {
                    node = node.Left;
                }

                return new KeyValuePair<TKey, TValue>(node.Key, node.Value);
            }
        }
        public KeyValuePair<TKey, TValue> Max
        {
            get
            {
                if (_root == null)
                {
                    throw new InvalidOperationException("the collection is empty");
                }

                Node node = _root;

                while (node.Left != null)
                {
                    node = node.Left;
                }

                return new KeyValuePair<TKey, TValue>(node.Key, node.Value);
            }
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TValue this[int index]
        {
            get
            {
                return GetNode(index).Value;
            }
            set
            {
                GetNode(index).Value = value;
            }
        }

        public RandomizedBinaryTree(IComparer<TKey> comparer = null)
        {
            Comparer = comparer ?? Comparer<TKey>.Default;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Node node = _root;

            while (node != null)
            {
                int cmp = Comparer.Compare(key, node.Key);

                if (cmp < 0)
                {
                    node = node.Left;
                }
                else if (cmp > 0)
                {
                    node = node.Right;
                }
                else
                {
                    value = node.Value;

                    return true;
                }
            }

            value = default(TValue);

            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }
        public void Add(TKey key, TValue value)
        {
            InternalAdd(key, value, ref _root);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }
        public bool Remove(TKey key)
        {
            ref Node nodeRef = ref FindNodeRef(key, ref _root);

            if (nodeRef == null)
                return false;

            nodeRef = Join(nodeRef.Left, nodeRef.Right);

            return true;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return TryGetValue(item.Key, out TValue value)
                   && EqualityComparer<TValue>.Default.Equals(item.Value, value);
        }

        public void Clear()
        {
            _root = null;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "arrayIndex must be non-negative");
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException("insufficient space", nameof(array) + ", " + nameof(arrayIndex));
            }

            int i = arrayIndex;

            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                array[i++] = kvp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Stack<Node> stack = new Stack<Node>();

            void PushLefts(Node node)
            {
                for (Node n = node; n != null; n = n.Left)
                {
                    stack.Push(n);
                }
            }

            PushLefts(_root);

            while (stack.Count > 0)
            {
                Node next = stack.Pop();

                yield return new KeyValuePair<TKey, TValue>(next.Key, next.Value);

                PushLefts(next.Right);
            }
        }

        private Node GetNode(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "index must be non-negative and less than Count");
            }

            Node node = _root;
            int nodeIndex = index;

            while (true)
            {
                int lefTCount = node?.Left?.Count ?? 0;

                if (nodeIndex < lefTCount)
                {
                    node = node?.Left;
                }
                else if (nodeIndex == lefTCount)
                {
                    return node;
                }
                else
                {
                    nodeIndex -= (lefTCount + 1);
                    node = node?.Right;
                }
            }
        }

        private void InternalAdd(TKey key, TValue value, ref Node nodeRef)
        {
            Node node = nodeRef;

            if (node == null)
            {
                nodeRef = new Node
                {
                    Key = key,
                    Value = value,
                    Count = 1
                };

                return;
            }

            if (Choose(node.Count))
            {
                (Node left, Node right) = Split(node, key);

                nodeRef = new Node
                {
                    Key = key,
                    Value = value,
                    Left = left,
                    Right = right,
                    Count = Node.ComputeCount(left, right)
                };

                return;
            }

            ++node.Count;

            int cmp = Comparer.Compare(key, node.Key);

            if (cmp < 0)
            {
                InternalAdd(key, value, ref node.Left);
            }
            else
            {
                InternalAdd(key, value, ref node.Right);
            }
        }

        private (Node Left, Node Right) Split(Node node, TKey key)
        {
            Node left, right;

            int cmp = Comparer.Compare(key, node.Key);

            if (cmp < 0)
            {
                right = node;

                if (node.Left == null)
                {
                    left = null;
                }
                else
                {
                    node.Count -= node.Left.Count;
                    (left, node.Left) = Split(node.Left, key);
                    node.Count += node.Left?.Count ?? 0;
                }
            }
            else
            {
                left = node;

                if (node.Right == null)
                {
                    right = null;
                }
                else
                {
                    node.Count -= node.Right.Count;
                    (node.Right, right) = Split(node.Left, key);
                    node.Count += node.Right?.Count ?? 0;
                }
            }

            return (left, right);
        }

        private ref Node FindNodeRef(TKey key, ref Node node)
        {
            if (node == null)
                return ref node;

            int cmp = Comparer.Compare(key, node.Key);

            if (cmp < 0)
            {
                ref Node result = ref FindNodeRef(key, ref node.Left);

                if (result != null)
                    --node.Count;

                return ref result;
            }
            else if (cmp > 0)
            {
                ref Node result = ref FindNodeRef(key, ref node.Right);

                if (result != null)
                    --node.Count;

                return ref result;
            }

            return ref node;
        }

        private Node Join(Node left, Node right)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            if (Choose(left.Count, right.Count))
            {
                left.Count += right.Count;
                left.Right = Join(left.Right, right);

                return left;
            }
            else
            {
                right.Count += left.Count;
                right.Left = Join(left, right.Left);

                return right;
            }
        }

        private bool Choose(int count)
        {
            return _random.NextDouble() < 1.0 / (count + 1);
        }
        private bool Choose(int lefTCount, int righTCount)
        {
            return _random.NextDouble() < lefTCount / (double)(lefTCount + righTCount);
        }

        private int InternalMaxDepth(Node node)
        {
            return node == null
                ? 0
                : 1 + Math.Max(InternalMaxDepth(node.Left), InternalMaxDepth(node.Right));
        }

        private void InternalCheckInvariants(Node node, Node parent, bool isNodeLefTChild)
        {
            HashSet<Node> nodes = new HashSet<Node>();

            if (node == null)
                return;

            if (!nodes.Add(node))
            {
                throw new InvalidOperationException($"node {node} encountered multiple times");
            }

            if (node.Left != null)
            {
                if (Comparer.Compare(node.Key, node.Left.Key) < 0)
                {
                    throw new InvalidOperationException($"node {node} should be >= left child {node.Left}");
                }

                if (parent != null
                    && !isNodeLefTChild
                    && Comparer.Compare(parent.Key, node.Left.Key) > 0)
                {
                    throw new InvalidOperationException($"left child {node.Left} should be between {parent} and {node}");
                }
            }

            if (node.Right != null)
            {
                if (Comparer.Compare(node.Key, node.Right.Key) > 0)
                {
                    throw new InvalidOperationException($"node {node} should be <= right child {node.Right}");
                }

                if (parent != null
                    && isNodeLefTChild
                    && Comparer.Compare(parent.Key, node.Right.Key) < 0)
                {
                    throw new InvalidOperationException($"right child {node.Right} should be between {node} and {parent}");
                }
            }

            InternalCheckInvariants(node.Left, node, true);
            InternalCheckInvariants(node.Right, node, false);

            if (node.Count != Node.ComputeCount(node.Left, node.Right))
            {
                throw new InvalidOperationException($"Bad count for {node} based on children {node.Left?.ToString() ?? "null"}, {node.Right?.ToString() ?? "null"}");
            }
        }

        internal int MaxDepth()
        {
            return InternalMaxDepth(_root);
        }

        internal void CheckInvariants()
        {
            InternalCheckInvariants(_root, null, false);
        }
    }
}
