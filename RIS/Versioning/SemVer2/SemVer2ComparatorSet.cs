// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Versioning
{
    internal class SemVer2ComparatorSet : IEquatable<SemVer2ComparatorSet>
    {
        private readonly List<SemVer2Comparator> _comparators;

        public SemVer2ComparatorSet(string pattern, bool allowZerosVersion = false)
        {
            _comparators = new List<SemVer2Comparator>();

            if (pattern is null)
                pattern = "*";

            pattern = pattern.Trim();

            if (pattern.Length == 0)
                pattern = "*";

            int position = 0;
            int startPosition = position;
            int endPosition = pattern.Length;

            while (position < endPosition)
            {
                foreach (Func<string, bool, (int? MatchLength, SemVer2Comparator[] Comparators)> comparatorFunc
                    in new Func<string, bool, (int? MatchLength, SemVer2Comparator[] Comparators)>[]
                    {
                        SemVer2ComparatorSetHelper.HyphenRange,
                        SemVer2ComparatorSetHelper.TildeRange,
                        SemVer2ComparatorSetHelper.CaretRange,
                        SemVer2ComparatorSetHelper.StarRange,
                    })
                {
                    (int? MatchLength, SemVer2Comparator[] Comparators) comparatorsResult = comparatorFunc(pattern.Substring(position), allowZerosVersion);

                    if (comparatorsResult != (null, null))
                    {
                        position += comparatorsResult.MatchLength.Value;
                        _comparators.AddRange(comparatorsResult.Comparators);
                    }
                }

                (int? MatchLength, SemVer2Comparator Comparator) comparatorResult = SemVer2Comparator.TryParse(pattern.Substring(position), allowZerosVersion);
                
                if (comparatorResult != (null, null))
                {
                    position += comparatorResult.MatchLength.Value;
                    _comparators.Add(comparatorResult.Comparator);
                }

                if (position == startPosition)
                {
                    var exception = new FormatException($"Недопустимый шаблон диапазона [{pattern}]");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
            }
        }
        private SemVer2ComparatorSet(IEnumerable<SemVer2Comparator> comparators)
        {
            _comparators = comparators.ToList();
        }

        public SemVer2ComparatorSet Intersect(SemVer2ComparatorSet other)
        {
            bool IsGreaterThan(SemVer2Comparator comparator)
            {
                return comparator.CompareOperator == CompareOperator.GreaterThan
                       || comparator.CompareOperator == CompareOperator.GreaterThanOrEqual;
            }
            
            bool IsLessThan(SemVer2Comparator comparator)
            {
                return comparator.CompareOperator == CompareOperator.LessThan
                       || comparator.CompareOperator == CompareOperator.LessThanOrEqual;
            }


            SemVer2Comparator maxOfMins = _comparators
                .Concat(other._comparators)
                .Where(IsGreaterThan)
                .OrderByDescending(comparator => comparator.Version).FirstOrDefault();

            SemVer2Comparator minOfMaxs = _comparators
                .Concat(other._comparators)
                .Where(IsLessThan)
                .OrderBy(comparator => comparator.Version).FirstOrDefault();

            if (maxOfMins != null && minOfMaxs != null && !maxOfMins.Intersect(minOfMaxs))
                return null;

            List<SemVer2> equalityVersions = _comparators
                .Concat(other._comparators)
                .Where(comparator => comparator.CompareOperator == CompareOperator.Equal)
                .Select(comparator => comparator.Version)
                .ToList();

            if (equalityVersions.Count > 1)
            {
                if (equalityVersions.Any(version => version != equalityVersions[0]))
                    return null;
            }
            if (equalityVersions.Count > 0)
            {
                if (maxOfMins != null && !maxOfMins.IsSatisfied(equalityVersions[0]))
                    return null;

                if (minOfMaxs != null && !minOfMaxs.IsSatisfied(equalityVersions[0]))
                    return null;

                return new SemVer2ComparatorSet(new List<SemVer2Comparator>
                {
                    new SemVer2Comparator(CompareOperator.Equal, equalityVersions[0])
                });
            }

            List<SemVer2Comparator> comparators = new List<SemVer2Comparator>();

            if (maxOfMins != null)
                comparators.Add(maxOfMins);

            if (minOfMaxs != null)
                comparators.Add(minOfMaxs);

            return comparators.Count > 0 ? new SemVer2ComparatorSet(comparators) : null;
        }

        public bool IsSatisfied(SemVer2 version)
        {
            bool satisfied = _comparators.All(comparator =>
                comparator.IsSatisfied(version));

            //if (version.IsPrereleaseIncluded)
            //{
            //    return satisfied 
            //           && _comparators.Any(comparator => 
            //               comparator.Version.IsPrereleaseIncluded
            //               && comparator.Version.Clone() == version.Clone());
            //}

            return satisfied;
        }

        public override int GetHashCode()
        {
            return _comparators.Aggregate(0, (hash, comparator) => hash ^ comparator.GetHashCode());
        }

        public override string ToString()
        {
            return string.Join(" ", _comparators.Select(comparator => comparator.ToString()).ToArray());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SemVer2ComparatorSet);
        }
        public bool Equals(SemVer2ComparatorSet obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            HashSet<SemVer2Comparator> comparators = new HashSet<SemVer2Comparator>(_comparators);

            return comparators.SetEquals(obj._comparators);
        }
    }
}
