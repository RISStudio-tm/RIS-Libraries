using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Versioning
{
    public class SemVer2Range : IEquatable<SemVer2Range>
    {
        private readonly SemVer2ComparatorSet[] _comparatorSets;
        private readonly string _pattern;

        public SemVer2Range(string pattern, bool allowZerosVersion = false)
        {
            if (pattern == null)
            {
                var exception = new ArgumentNullException(nameof(pattern), $"{nameof(pattern)} не должен быть равен null");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            _pattern = pattern;
            string[] comparatorSetPatterns = pattern.Split(new [] {"||"}, StringSplitOptions.None);
            _comparatorSets = comparatorSetPatterns.Select(comparatorSetPattern => new SemVer2ComparatorSet(comparatorSetPattern, allowZerosVersion)).ToArray();
        }
        private SemVer2Range(IEnumerable<SemVer2ComparatorSet> comparatorSets)
        {
            _comparatorSets = comparatorSets.ToArray();
            _pattern = string.Join(" || ", _comparatorSets.Select(semVer2ComparatorSet => semVer2ComparatorSet.ToString()).ToArray());
        }

        private static IEnumerable<SemVer2> ValidVersions(IEnumerable<string> versions, bool allowZerosVersion)
        {
            foreach (var version in versions)
            {
                SemVer2 result;

                try
                {
                    result = new SemVer2(version, allowZerosVersion);
                }
                catch (Exception)
                {
                    continue;
                }

                yield return result;
            }
        }

        public SemVer2Range Intersect(SemVer2Range other)
        {
            List<SemVer2ComparatorSet> allIntersections = _comparatorSets
                .SelectMany(semVer2ComparatorSet => other._comparatorSets.Select(semVer2ComparatorSet.Intersect))
                .Where(semVer2ComparatorSet => semVer2ComparatorSet != null)
                .ToList();

            if (allIntersections.Count == 0)
                return new SemVer2Range("<0.0.0", true);

            return new SemVer2Range(allIntersections);
        }

        public static bool IsSatisfied(string pattern, string version, bool allowZerosVersion = false)
        {
            SemVer2Range range = new SemVer2Range(pattern);

            return range.IsSatisfied(version, allowZerosVersion);
        }
        public bool IsSatisfied(string version, bool allowZerosVersion = false)
        {
            try
            {
                SemVer2 result = new SemVer2(version, allowZerosVersion);

                return IsSatisfied(result);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsSatisfied(SemVer2 version)
        {
            return _comparatorSets.Any(semVer2ComparatorSet => semVer2ComparatorSet.IsSatisfied(version));
        }

        public static IEnumerable<string> Satisfying(string pattern, IEnumerable<string> versions, bool allowZerosVersion = false)
        {
            SemVer2Range range = new SemVer2Range(pattern);

            return range.Satisfying(versions, allowZerosVersion);
        }
        public IEnumerable<string> Satisfying(IEnumerable<string> versions, bool allowZerosVersion = false)
        {
            return versions.Where(version => IsSatisfied(version, allowZerosVersion));
        }
        public IEnumerable<SemVer2> Satisfying(IEnumerable<SemVer2> versions)
        {
            return versions.Where(version => IsSatisfied(version));
        }

        public static string MaxSatisfying(string pattern, IEnumerable<string> versions, bool allowZerosVersion = false)
        {
            SemVer2Range range = new SemVer2Range(pattern);

            return range.MaxSatisfying(versions, allowZerosVersion);
        }
        public string MaxSatisfying(IEnumerable<string> versions, bool allowZerosVersion = false)
        {
            IEnumerable<SemVer2> validVersions = ValidVersions(versions, allowZerosVersion);
            SemVer2 maxVersion = MaxSatisfying(validVersions);

            return maxVersion?.ToString();
        }
        public SemVer2 MaxSatisfying(IEnumerable<SemVer2> versions)
        {
            return Satisfying(versions).Max();
        }

        public override int GetHashCode()
        {
            return _comparatorSets.Aggregate(0, (hash, comparatorSet) => hash ^ comparatorSet.GetHashCode());
        }

        public override string ToString()
        {
            return _pattern;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SemVer2Range);
        }
        public bool Equals(SemVer2Range obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            HashSet<SemVer2ComparatorSet> comparatorSets = new HashSet<SemVer2ComparatorSet>(_comparatorSets);

            return comparatorSets.SetEquals(obj._comparatorSets);
        }

        public static bool operator ==(SemVer2Range a, SemVer2Range b)
        {
            if (a is null)
            {
                return b is null;
            }

            return a.Equals(b);
        }
        public static bool operator !=(SemVer2Range a, SemVer2Range b)
        {
            return !(a == b);
        }
    }
}
