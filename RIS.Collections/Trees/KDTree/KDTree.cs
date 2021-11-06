// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Collections.Trees
{
    // todo Rebalance(bool force)
    public sealed class KDTree<TPoint, TValue>
    {
        private sealed class Node
        {
            internal TPoint point;
            internal TValue value;
            internal Node left, right;
        }
        private sealed class PointAndValueComparer : IComparer<KeyValuePair<TPoint, TValue>>
        {
            private readonly IKDPointComparer<TPoint> _comparer;
            private readonly int _dimension;

            public PointAndValueComparer(IKDPointComparer<TPoint> comparer, int dimension)
            {
                _comparer = comparer;
                _dimension = dimension;
            }

            int IComparer<KeyValuePair<TPoint, TValue>>.Compare(KeyValuePair<TPoint, TValue> x, KeyValuePair<TPoint, TValue> y)
            {
                return _comparer.Compare(x.Key, y.Key, _dimension);
            }
        }

        private readonly IKDPointComparer<TPoint> _comparer;
        private PointAndValueComparer[] _singleDimensionComparers;
        private Node _root;

        public int Count { get; private set; }

        public KDTree(IKDPointComparer<TPoint> comparer = null)
        {
            _comparer = comparer;
        }
        public KDTree(IEnumerable<KeyValuePair<TPoint, TValue>> points, IKDPointComparer<TPoint> comparer)
            : this(comparer)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            KeyValuePair<TPoint, TValue>[] pointsArray = points.ToArray();

            if (pointsArray.Any(kvp => kvp.Key == null))
            {
                throw new ArgumentNullException(nameof(points), "keys must be non-null");
            }

            if (pointsArray.Length > 0)
            {
                EnsureInitialized(pointsArray[0].Key);

                _root = BuildTree(pointsArray, 0, pointsArray.Length - 1, 0);
                Count = pointsArray.Length;
            }
        }

        public void Add(TPoint point, TValue value)
        {
            if (point == null)
            {
                throw new ArgumentNullException(nameof(point));
            }

            EnsureInitialized(point);

            InternalAdd(ref _root, point, value, 0);

            ++Count;
        }

        public bool Remove(TPoint point, TValue value)
        {
            if (point == null)
            {
                throw new ArgumentNullException(nameof(point));
            }

            if (InternalRemove(ref _root, point, value, 0))
            {
                --Count;

                return true;
            }

            return false;
        }

        public TAccumulate AggregateRange<TAccumulate>(TPoint lowerBounds, TPoint upperBounds, TAccumulate seed, Func<TAccumulate, KeyValuePair<TPoint, TValue>, TAccumulate> accumulator)
        {
            if (lowerBounds == null)
            {
                throw new ArgumentNullException(nameof(lowerBounds));
            }

            if (upperBounds == null)
            {
                throw new ArgumentNullException(nameof(upperBounds));
            }

            if (accumulator == null)
            {
                throw new ArgumentNullException(nameof(accumulator));
            }

            EnsureInitialized(lowerBounds);

            return InternalAggregateRange(_root, lowerBounds, upperBounds, seed, accumulator, 0);
        }

        private void InternalAdd(ref Node node, TPoint point, TValue value, int dimension)
        {
            if (node == null)
                node = new Node { point = point, value = value };
            else if (_comparer.Compare(point, node.point, dimension) <= 0)
                InternalAdd(ref node.left, point, value, NextDimension(dimension));
            else
                InternalAdd(ref node.right, point, value, NextDimension(dimension));
        }

        private bool InternalRemove(ref Node node, TPoint point, TValue value, int dimension)
        {
            if (node == null)
                return false;

            int cmp = _comparer.Compare(node.point, point, dimension);

            if (cmp < 0)
                return InternalRemove(ref node.left, point, value, NextDimension(dimension));

            if (cmp > 0)
                return InternalRemove(ref node.right, point, value, NextDimension(dimension));

            if (PointEquals(node.point, point) && EqualityComparer<TValue>.Default.Equals(node.value, value))
            {
                if (node.left == null && node.right == null)
                {
                    node = null;
                }
                else
                {
                    Node replacementNode = FindReplacemenTChildForRemoval(node, dimension, out int replacementDimension);

                    node.point = replacementNode.point;
                    node.value = replacementNode.value;

                    // have to remove the replacement. If it's a leaf we have to re-search for it from here (grr), otherwise
                    // we can just do this operation again since it can only modify the node
                    throw new NotImplementedException();
                }

                return true;
            }

            var nextDimension = NextDimension(dimension);
            return InternalRemove(ref node.left, point, value, nextDimension)
                || InternalRemove(ref node.right, point, value, nextDimension);
        }

        private Node FindReplacemenTChildForRemoval(Node toReplace, int toReplaceDimension, out int replacementDimension)
        {
            Node best;
            int bestDimension = NextDimension(toReplaceDimension);

            if (toReplace.left != null)
            {
                best = toReplace.left;

                LeftFindReplacemenTChildForRemoval(toReplaceDimension, best, bestDimension, ref best, ref bestDimension);
            }
            else
            {
                best = toReplace.right;

                RightFindReplacemenTChildForRemoval(toReplaceDimension, best, bestDimension, ref best, ref bestDimension);
            }

            replacementDimension = bestDimension;

            return best;
        }

        private void LeftFindReplacemenTChildForRemoval(int toReplaceDimension, Node current, int currentDimension,
            ref Node best, ref int bestDimension)
        {
            if (current == null)
                return;

            int cmp = _comparer.Compare(current.point, best.point, toReplaceDimension);

            if (cmp >= 0)
            {
                best = current;
                bestDimension = currentDimension;
            }

            int nextDimension = NextDimension(currentDimension);

            LeftFindReplacemenTChildForRemoval(toReplaceDimension, current.right, nextDimension, ref best, ref bestDimension);

            if (currentDimension != toReplaceDimension)
                LeftFindReplacemenTChildForRemoval(toReplaceDimension, current.left, nextDimension, ref best, ref bestDimension);
        }

        private void RightFindReplacemenTChildForRemoval(int toReplaceDimension, Node current, int currentDimension,
            ref Node best, ref int bestDimension)
        {
            if (current == null)
                return;

            int cmp = _comparer.Compare(current.point, best.point, toReplaceDimension);

            if (cmp <= 0)
            {
                best = current;
                bestDimension = currentDimension;
            }

            int nextDimension = NextDimension(currentDimension);

            RightFindReplacemenTChildForRemoval(toReplaceDimension, current.left, nextDimension, ref best, ref bestDimension);

            if (currentDimension != toReplaceDimension)
                RightFindReplacemenTChildForRemoval(toReplaceDimension, current.right, nextDimension, ref best, ref bestDimension);
        }

        private TAccumulate InternalAggregateRange<TAccumulate>(Node node, TPoint lowerBounds, TPoint upperBounds, TAccumulate seed, Func<TAccumulate, KeyValuePair<TPoint, TValue>, TAccumulate> accumulator, int dimension)
        {
            if (node == null)
                return seed;

            if (_comparer.Compare(node.point, lowerBounds, dimension) < 0)
                return InternalAggregateRange(node.left, lowerBounds, upperBounds, seed, accumulator, NextDimension(dimension));

            if (_comparer.Compare(node.point, upperBounds, dimension) > 0)
                return InternalAggregateRange(node.right, lowerBounds, upperBounds, seed, accumulator, NextDimension(dimension));

            TAccumulate withNode = accumulator(seed, new KeyValuePair<TPoint, TValue>(node.point, node.value));
            int nextDimension = NextDimension(dimension);
            TAccumulate withNodeAndLeft = InternalAggregateRange(node.left, lowerBounds, upperBounds, withNode, accumulator, nextDimension);

            return InternalAggregateRange(node.right, lowerBounds, upperBounds, withNodeAndLeft, accumulator, nextDimension);
        }

        private bool PointEquals(TPoint a, TPoint b)
        {
            int dimensions = _comparer.Dimensions;

            for (int dimension = 0; dimension < dimensions; ++dimension)
            {
                if (_comparer.Compare(a, b, dimension) != 0)
                    return false;
            }

            return true;
        }

        private void EnsureInitialized(TPoint point)
        {
            if (_singleDimensionComparers != null)
                return;

            (_comparer as IInitializableKDPointComparer<TPoint>)?.InitializeFrom(point);
            _singleDimensionComparers = Enumerable.Range(0, _comparer.Dimensions)
                .Select(i => new PointAndValueComparer(_comparer, i))
                .ToArray();
        }

        private Node BuildTree(KeyValuePair<TPoint, TValue>[] points, int left, int right, int dimension)
        {
            if (right - left < 0)
                return null;

            if (left == right)
                return new Node { point = points[left].Key, value = points[left].Value };

            int median = KDTreeSelector.Select(points, left, right, left + ((right - left) / 2), _singleDimensionComparers[dimension]);
            int nextDimension = NextDimension(dimension);

            return new Node
            {
                point = points[median].Key,
                value = points[median].Value,
                left = BuildTree(points, left, median - 1, nextDimension),
                right = BuildTree(points, median + 1, right, nextDimension),
            };
        }

        private int NextDimension(int dimension)
        {
            return dimension == _comparer.Dimensions ? 0 : dimension + 1;
        }
    }
}
