// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text.RegularExpressions;
using RIS.Mathematics;

namespace RIS.Versioning
{
    internal static class SemVer2ComparatorSetHelper
    {
        private const string VERSION_CHARS = @"[0-9a-zA-Z\-\+\.\*]";

        public static (int? MatchLength, SemVer2Comparator[] Comparators) TildeRange(string pattern, bool allowZerosVersion = false)
        {
            Regex regex = new Regex($@"^\s*[~]\s*(?<version>{VERSION_CHARS}+)\s*", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
            Match match = regex.Match(pattern);

            if (!match.Success)
                return (null, null);

            PartialSemVer2 version;
            SemVer2 minVersion;
            SemVer2 maxVersion;

            try
            {
                version = new PartialSemVer2(match.Groups["version"].Value, allowZerosVersion);
            }
            catch (FormatException)
            {
                return (null, null);
            }

            minVersion = version.ToSemVer2(allowZerosVersion);

            if (!version.Major.HasValue)
            {
                maxVersion = null;
            }
            else if (!version.Minor.HasValue)
            {
                maxVersion = new SemVer2(version.Major.Value + 1, 0, 0, allowZerosVersion);
            }
            else
            {
                maxVersion = new SemVer2(version.Major.Value, version.Minor.Value + 1, 0, allowZerosVersion);
            }

            return (match.Length, MinMaxComparators(minVersion, maxVersion));
        }

        public static (int? MatchLength, SemVer2Comparator[] Comparators) CaretRange(string pattern, bool allowZerosVersion = false)
        {
            Regex regex = new Regex($@"^\s*[\^]\s*(?<version>{VERSION_CHARS}+)\s*", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
            Match match = regex.Match(pattern);

            if (!match.Success)
                return (null, null);

            PartialSemVer2 version;
            SemVer2 minVersion;
            SemVer2 maxVersion;

            try
            {
                version = new PartialSemVer2(match.Groups["version"].Value, allowZerosVersion);
            }
            catch (FormatException)
            {
                return (null, null);
            }

            minVersion = version.ToSemVer2(allowZerosVersion);

            if (!version.Major.HasValue)
            {
                maxVersion = null;
            }
            else if (version.Major.Value > 0 || !version.Minor.HasValue)
            {
                maxVersion = new SemVer2(version.Major.Value + 1, 0, 0, allowZerosVersion);
            }
            else if (version.Minor.Value > 0 || !version.Patch.HasValue)
            {
                maxVersion = new SemVer2(0, version.Minor.Value + 1, 0, allowZerosVersion);
            }
            else
            {
                maxVersion = new SemVer2(0, 0, version.Patch.Value + 1, allowZerosVersion);
            }

            return (match.Length, MinMaxComparators(minVersion, maxVersion));
        }

        public static (int? MatchLength, SemVer2Comparator[] Comparators) HyphenRange(string pattern, bool allowZerosVersion = false)
        {
            Regex regex = new Regex($@"^\s*(?<version_1>{VERSION_CHARS}+)\s+\[-]\s+(?<version_2>{VERSION_CHARS}+)\s*", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
            Match match = regex.Match(pattern);

            if (!match.Success)
                return (null, null);

            PartialSemVer2 minPartialSemVer2;
            PartialSemVer2 maxPartialSemVer2;
            SemVer2 minVersion;
            SemVer2 maxVersion;

            try
            {
                minPartialSemVer2 = new PartialSemVer2(match.Groups["version_1"].Value, allowZerosVersion);
                maxPartialSemVer2 = new PartialSemVer2(match.Groups["version_2"].Value, allowZerosVersion);
            }
            catch (FormatException)
            {
                return (null, null);
            }

            minVersion = minPartialSemVer2.ToSemVer2(allowZerosVersion);
            CompareOperator maxOperator = maxPartialSemVer2.IsFullRelease ? CompareOperator.LessThanOrEqual : CompareOperator.LessThan;

            if (!maxPartialSemVer2.Major.HasValue)
            {
                maxVersion = null;
            }
            else if (!maxPartialSemVer2.Minor.HasValue)
            {
                maxVersion = new SemVer2(maxPartialSemVer2.Major.Value + 1, 0, 0, allowZerosVersion);
            }
            else if (!maxPartialSemVer2.Patch.HasValue)
            {
                maxVersion = new SemVer2(maxPartialSemVer2.Major.Value, maxPartialSemVer2.Minor.Value + 1, 0, allowZerosVersion);
            }
            else
            {
                maxVersion = maxPartialSemVer2.ToSemVer2(allowZerosVersion);
            }

            return (match.Length, MinMaxComparators(minVersion, maxVersion, maxOperator));
        }

        public static (int? MatchLength, SemVer2Comparator[] Comparators) StarRange(string pattern, bool allowZerosVersion = false)
        {
            Regex regex = new Regex($@"^\s*[=]?\s*(?<version>{VERSION_CHARS}+)\s*", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
            Match match = regex.Match(pattern);

            if (!match.Success)
                return (null, null);

            PartialSemVer2 version;
            SemVer2 minVersion;
            SemVer2 maxVersion;

            try
            {
                version = new PartialSemVer2(match.Groups["version"].Value, allowZerosVersion);
            }
            catch (FormatException)
            {
                return (null, null);
            }

            if (version.IsFullRelease)
                return (null, null);

            minVersion = version.ToSemVer2(allowZerosVersion);

            if (!version.Major.HasValue)
            {
                maxVersion = null;
            }
            else if (!version.Minor.HasValue)
            {
                maxVersion = new SemVer2(version.Major.Value + 1, 0, 0, allowZerosVersion);
            }
            else
            {
                maxVersion = new SemVer2(version.Major.Value, version.Minor.Value + 1, 0, allowZerosVersion);
            }

            return (match.Length, MinMaxComparators(minVersion, maxVersion));
        }

        private static SemVer2Comparator[] MinMaxComparators(SemVer2 minVersion, SemVer2 maxVersion,
            CompareOperator maxOperator = CompareOperator.LessThan)
        {
            SemVer2Comparator minComparator = new SemVer2Comparator(CompareOperator.GreaterThanOrEqual, minVersion);

            if (maxVersion == null)
            {
                return new [] { minComparator };
            }
            else
            {
                SemVer2Comparator maxComparator = new SemVer2Comparator(maxOperator, maxVersion);

                return new [] { minComparator, maxComparator };
            }
        }
    }
}
