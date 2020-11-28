// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text.RegularExpressions;

namespace RIS.Versioning
{
    internal class SemVer2Comparator : IEquatable<SemVer2Comparator>
    {
        public static readonly Regex CompareInfoRegex = new Regex(@"^\s*(?<compare_operator>(?:[<>!]?[=]?))\s*(?<version>[0-9a-zA-Z\-\+\.\*]+)\s*$", RegexOptions.Multiline, TimeSpan.FromSeconds(5));

        public readonly CompareOperator CompareOperator;
        public readonly SemVer2 Version;

        public SemVer2Comparator(string pattern, bool allowZerosVersion = false)
        {
            if (pattern == null)
            {
                var exception = new ArgumentNullException(nameof(pattern), $"{nameof(pattern)} не должен быть равен null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            Match match = CompareInfoRegex.Match(pattern);

            if (!match.Success)
            {
                var exception = new FormatException($"Недопустимый шаблон компаратора [{pattern}]");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            CompareOperator = StringToCompareOperator(match.Groups["compare_operator"].Value);
            PartialSemVer2 partialVersion = new PartialSemVer2(match.Groups["version"].Value, allowZerosVersion);

            if (!partialVersion.IsFullRelease)
            {
                switch (CompareOperator)
                {
                    case CompareOperator.LessThanOrEqual:
                        CompareOperator = CompareOperator.LessThan;
                        if (!partialVersion.Major.HasValue)
                        {
                            CompareOperator = CompareOperator.GreaterThanOrEqual;
                            Version = new SemVer2(0, 0, 0, true);
                        }
                        else if (!partialVersion.Minor.HasValue)
                        {
                            Version = new SemVer2(partialVersion.Major.Value + 1, 0, 0, allowZerosVersion);
                        }
                        else
                        {
                            Version = new SemVer2(partialVersion.Major.Value, partialVersion.Minor.Value + 1, 0, allowZerosVersion);
                        }
                        break;
                    case CompareOperator.GreaterThan:
                        CompareOperator = CompareOperator.GreaterThanOrEqual;
                        if (!partialVersion.Major.HasValue)
                        {
                            CompareOperator = CompareOperator.LessThan;
                            Version = new SemVer2(0, 0, 0, true);
                        }
                        else if (!partialVersion.Minor.HasValue)
                        {
                            Version = new SemVer2(partialVersion.Major.Value + 1, 0, 0, allowZerosVersion);
                        }
                        else
                        {
                            Version = new SemVer2(partialVersion.Major.Value, partialVersion.Minor.Value + 1, 0, allowZerosVersion);
                        }
                        break;
                    default:
                        Version = partialVersion.ToSemVer2(allowZerosVersion);
                        break;
                }
            }
            else
            {
                Version = partialVersion.ToSemVer2(allowZerosVersion);
            }
        }
        public SemVer2Comparator(CompareOperator comparatorType, SemVer2 version)
        {
            if (version == null)
            {
                var exception = new ArgumentNullException(nameof(version), $"{nameof(version)} не должен быть равен null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            CompareOperator = comparatorType;
            Version = version;
        }

        private static CompareOperator StringToCompareOperator(string pattern)
        {
            switch (pattern)
            {
                case (""):
                case ("="):
                    return CompareOperator.Equal;
                case ("!="):
                    return CompareOperator.NotEqual;
                case ("<"):
                    return CompareOperator.LessThan;
                case ("<="):
                    return CompareOperator.LessThanOrEqual;
                case (">"):
                    return CompareOperator.GreaterThan;
                case (">="):
                    return CompareOperator.GreaterThanOrEqual;
                default:
                    var exception = new FormatException($"Недопустимый оператор сравнения компаратора [{pattern}]");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
            }
        }
        private static string CompareOperatorToString(CompareOperator compareOperator)
        {
            switch (compareOperator)
            {
                case (CompareOperator.Equal):
                    return "=";
                case (CompareOperator.NotEqual):
                    return "!=";
                case (CompareOperator.LessThan):
                    return "<";
                case (CompareOperator.LessThanOrEqual):
                    return "<=";
                case (CompareOperator.GreaterThan):
                    return ">";
                case (CompareOperator.GreaterThanOrEqual):
                    return ">=";
                default:
                    var exception = new FormatException($"Недопустимый оператор сравнения компаратора [{compareOperator}]");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
            }
        }

        public static (int? MatchLength, SemVer2Comparator Comparator) TryParse(string pattern, bool allowZerosVersion = false)
        {
            var match = CompareInfoRegex.Match(pattern);

            if (!match.Success)
                return (null, null);

            return (match.Length, new SemVer2Comparator(match.Value, allowZerosVersion));
        }

        public bool Intersect(SemVer2Comparator other)
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

            bool IsEqual(SemVer2Comparator comparator)
            {
                return comparator.CompareOperator == CompareOperator.GreaterThanOrEqual
                       || comparator.CompareOperator == CompareOperator.Equal
                       || comparator.CompareOperator == CompareOperator.LessThanOrEqual;
            }

            bool IsNotEqual(SemVer2Comparator comparator)
            {
                return comparator.CompareOperator == CompareOperator.GreaterThan
                    || comparator.CompareOperator == CompareOperator.NotEqual
                    || comparator.CompareOperator == CompareOperator.LessThan;
            }


            if (Version > other.Version && (IsLessThan(this) || IsGreaterThan(other)))
                return true;

            if (Version < other.Version && (IsGreaterThan(this) || IsLessThan(other)))
                return true;

            if (Version == other.Version 
                && ((IsEqual(this) && IsEqual(other))
                    || (IsLessThan(this) && IsLessThan(other))
                    || (IsGreaterThan(this) && IsGreaterThan(other))))
                return true;

            //if (Version != other.Version
            //    && ((IsNotEqual(this) && !IsNotEqual(other))
            //        || (IsEqual(this) && !IsEqual(other))
            //        || (IsLessThan(this) && !IsLessThan(other))
            //        || (IsGreaterThan(this) && !IsGreaterThan(other))))
            //    return true;

            //if (Version != other.Version && (IsNotEqual(this) && IsNotEqual(other)))
            //    return true;

            return false;
        }

        public bool IsSatisfied(SemVer2 version)
        {
            switch(CompareOperator)
            {
                case(CompareOperator.Equal):
                    return version == Version;
                case (CompareOperator.NotEqual):
                    return version != Version;
                case (CompareOperator.LessThan):
                    return version < Version;
                case(CompareOperator.LessThanOrEqual):
                    return version <= Version;
                case(CompareOperator.GreaterThan):
                    return version > Version;
                case(CompareOperator.GreaterThanOrEqual):
                    return version >= Version;
                default:
                    var exception = new ArgumentNullException(nameof(CompareOperator), $"Недопустимый оператор сравнения компаратора [{CompareOperator}]");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"{CompareOperatorToString(CompareOperator)}{Version}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SemVer2Comparator);
        }
        public bool Equals(SemVer2Comparator obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return CompareOperator == obj.CompareOperator && Version == obj.Version;
        }
    }
}
