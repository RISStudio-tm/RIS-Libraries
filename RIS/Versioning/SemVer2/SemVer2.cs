// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using RIS.Extensions;

namespace RIS.Versioning
{
    /// <summary>
    ///     Semantic version implementation. Conforms with v2.0.0 of http://semver.org.
    /// </summary>
    public sealed class SemVer2 : IComparable<SemVer2>, IComparable
    {
        public static readonly Regex VersionInfoAllowZerosVersionRegex = new Regex(@"^(?:[v][\.]?)?(?<major>(?:[0]|[1-9][0-9]*))[\.](?<minor>(?:[0]|[1-9][0-9]*))[\.](?<patch>(?:[0]|[1-9][0-9]*))(?:[-]?(?(?<=[-])(?<prerelease>(?:(?<prerelease_part>(?(?=[0](?:[\.]|[\+]|$))[0]|[A-Za-z1-9][A-Za-z0-9]*))(?(?=(?:[\.]$|[\.][\+]))(?!)|[\.])?)+)|(?:$|[\+])))(?:[\+]?(?(?<=[+])(?<metadata>(?:(?<metadata_part>[A-Za-z0-9]+)(?(?=[\.]$)(?!)|[\.])?)+)|$))$", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
        public static readonly Regex VersionInfoRegex = new Regex(@"^(?:[v][\.]?)?(?<major>(?:[0]|[1-9][0-9]*))[\.](?<minor>(?:[0]|[1-9][0-9]*))[\.](?<patch>(?(?<=(?:[1-9][\.]|[1-9][\.][0-9]+[\.]))(?:[0]|[1-9][0-9]*)|[1-9][0-9]*))(?:[-]?(?(?<=[-])(?<prerelease>(?:(?<prerelease_part>(?(?=[0](?:[\.]|[\+]|$))[0]|[A-Za-z1-9][A-Za-z0-9]*))(?(?=(?:[\.]$|[\.][\+]))(?!)|[\.])?)+)|(?:$|[\+])))(?:[\+]?(?(?<=[+])(?<metadata>(?:(?<metadata_part>[A-Za-z0-9]+)(?(?=[\.]$)(?!)|[\.])?)+)|$))$", RegexOptions.Multiline, TimeSpan.FromSeconds(5));

        public uint Major { get; }
        public uint Minor { get; }
        public uint Patch { get; }
        public bool IsPrereleaseIncluded { get; }
        public string Prerelease { get; }
        private readonly string[] _prereleasePartsArray;
        public ReadOnlyCollection<string> PrereleaseParts
        {
            get
            {
                return new ReadOnlyCollection<string>(_prereleasePartsArray);
            }
        }
        public bool IsMetadataIncluded { get; }
        public string Metadata { get; }
        private readonly string[] _metadataPartsArray;
        public ReadOnlyCollection<string> MetadataParts
        {
            get
            {
                return new ReadOnlyCollection<string>(_metadataPartsArray);
            }
        }

        public SemVer2(int major, int minor, int patch, bool allowZerosVersion = false)
        {
            if (major < 0)
                major = 0;

            if (minor < 0)
                minor = 0;

            if (patch < 0)
                patch = allowZerosVersion ? 0 : 1;

            if (!allowZerosVersion && major == 0 && minor == 0 && patch == 0)
            {
                var exception = new FormatException($"Не удалось распознать формат Semanic Version 2.0.0 (без поддержки нулевой версии и без поддержки wildcards) в строке [{major}.{minor}.{patch}]");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Major = (uint)major;
            Minor = (uint)minor;
            Patch = (uint)patch;
        }
        public SemVer2(int major, int minor, int patch, string prerelease, bool allowZerosVersion = false)
            : this($"{(major < 0 ? 0 : major)}.{(minor < 0 ? 0 : minor)}.{(patch < 0 ? (allowZerosVersion ? 0 : 1) : patch)}{(string.IsNullOrEmpty(prerelease) ? string.Empty : "-")}{prerelease ?? string.Empty}", allowZerosVersion)
        {

        }
        public SemVer2(int major, int minor, int patch, string prerelease, string metadata, bool allowZerosVersion = false)
            : this($"{(major < 0 ? 0 : major)}.{(minor < 0 ? 0 : minor)}.{(patch < 0 ? (allowZerosVersion ? 0 : 1) : patch)}{(string.IsNullOrEmpty(prerelease) ? string.Empty : "-")}{prerelease ?? string.Empty}{(string.IsNullOrEmpty(metadata) ? string.Empty : "+")}{metadata ?? string.Empty}", allowZerosVersion)
        {

        }
        public SemVer2(uint major, uint minor, uint patch, bool allowZerosVersion = false)
        {
            if (!allowZerosVersion && major == 0 && minor == 0 && patch == 0)
            {
                var exception = new FormatException($"Не удалось распознать формат Semanic Version 2.0.0 (без поддержки нулевой версии и без поддержки wildcards) в строке [{major}.{minor}.{patch}]");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Major = major;
            Minor = minor;
            Patch = patch;
        }
        public SemVer2(uint major, uint minor, uint patch, string prerelease, bool allowZerosVersion = false)
            : this($"{major}.{minor}.{patch}{(string.IsNullOrEmpty(prerelease) ? string.Empty : "-")}{prerelease ?? string.Empty}", allowZerosVersion)
        {

        }
        public SemVer2(uint major, uint minor, uint patch, string prerelease, string metadata, bool allowZerosVersion = false)
            : this($"{major}.{minor}.{patch}{(string.IsNullOrEmpty(prerelease) ? string.Empty : "-")}{prerelease ?? string.Empty}{(string.IsNullOrEmpty(metadata) ? string.Empty : "+")}{metadata ?? string.Empty}", allowZerosVersion)
        {

        }
        public SemVer2(string version, bool allowZerosVersion = false)
        {
            if (version == null)
            {
                var exception = new ArgumentNullException(nameof(version), $"{nameof(version)} не должен быть равен null");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            version = version.Trim();

            Match match = allowZerosVersion ? VersionInfoAllowZerosVersionRegex.Match(version) : VersionInfoRegex.Match(version);

            if (!match.Success)
            {
                var exception = allowZerosVersion
                    ? new FormatException($"Не удалось распознать формат Semanic Version 2.0.0 (с поддержкой нулевой версии и без поддержки wildcards) в строке [{version}]")
                    : new FormatException($"Не удалось распознать формат Semanic Version 2.0.0 (без поддержки нулевой версии и без поддержки wildcards) в строке [{version}]");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Major = match.Groups["major"].Value.ToUInt();
            Minor = match.Groups["minor"].Value.ToUInt();
            Patch = match.Groups["patch"].Value.ToUInt();

            IsPrereleaseIncluded = match.Groups["prerelease"].Success;

            if (IsPrereleaseIncluded)
            {
                Prerelease = match.Groups["prerelease"].Value;
                _prereleasePartsArray = new string[match.Groups["prerelease_part"].Captures.Count];

                for (int i = 0; i < match.Groups["prerelease_part"].Captures.Count; ++i)
                {
                    _prereleasePartsArray[i] = match.Groups["prerelease_part"].Captures[i].Value;
                }
            }
            else
            {
                Prerelease = string.Empty;
                _prereleasePartsArray = Array.Empty<string>();
            }

            IsMetadataIncluded = match.Groups["metadata"].Success;

            if (IsMetadataIncluded)
            {
                Metadata = match.Groups["metadata"].Value;
                _metadataPartsArray = new string[match.Groups["metadata_part"].Captures.Count];

                for (int i = 0; i < match.Groups["metadata_part"].Captures.Count; ++i)
                {
                    _metadataPartsArray[i] = match.Groups["metadata_part"].Captures[i].Value;
                }
            }
            else
            {
                Metadata = string.Empty;
                _metadataPartsArray = Array.Empty<string>();
            }
        }
        public SemVer2(Version version)
        {
            if (version == null)
            {
                var exception = new ArgumentNullException(nameof(version), $"{nameof(version)} не должен быть равен null");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Major = (uint)version.Major;
            Minor = (uint)version.Minor;

            if (version.Revision > 0)
                Patch = (uint)version.Revision;
            else
                Patch = 0u;

            IsPrereleaseIncluded = false;
            Prerelease = string.Empty;
            _prereleasePartsArray = Array.Empty<string>();

            IsMetadataIncluded = version.Build > 0;
            Metadata = IsMetadataIncluded ? version.Build.ToString() : string.Empty;
            _metadataPartsArray = IsMetadataIncluded ? new string[] { version.Build.ToString() } : Array.Empty<string>();
        }

        public static SemVer2 ZeroVersion()
        {
            return new SemVer2(0, 0, 0, true);
        }

        public SemVer2 Clone(bool copyPrerelease = false, bool copyMetadata = false)
        {
            if (!copyPrerelease && !copyMetadata)
                return new SemVer2(Major, Minor, Patch);

            return new SemVer2(Major, Minor, Patch, copyPrerelease ? Prerelease : null, copyMetadata ? Metadata : null, true);
        }

        public SemVer2 Change(uint? major = null, uint? minor = null, uint? patch = null, bool copyPrerelease = false, bool copyMetadata = false)
        {
            return new SemVer2(major ?? Major, minor ?? Minor, patch ?? Patch, copyPrerelease ? Prerelease : null, copyMetadata ? Metadata : null, true);
        }

        public SemVer2 IncrementMajor(bool copyPrerelease = false, bool copyMetadata = true)
        {
            return new SemVer2(Major + 1, 0, 0, copyPrerelease ? Prerelease : null, copyMetadata ? Metadata : null, true);
        }
        public SemVer2 IncrementMinor(bool copyPrerelease = false, bool copyMetadata = true)
        {
            return new SemVer2(Major, Minor + 1, 0, copyPrerelease ? Prerelease : null, copyMetadata ? Metadata : null, true);
        }
        public SemVer2 IncrementPatch(bool copyPrerelease = false, bool copyMetadata = true)
        {
            return new SemVer2(Major, Minor, Patch + 1, copyPrerelease ? Prerelease : null, copyMetadata ? Metadata : null, true);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                uint hashCode = Major;
                hashCode = (hashCode * 397) ^ Minor;
                hashCode = (hashCode * 397) ^ Patch;
                hashCode = (uint)((hashCode * 397) ^ (IsPrereleaseIncluded ? Prerelease.GetHashCode() : 0));
                hashCode = (uint)((hashCode * 397) ^ (IsMetadataIncluded ? Metadata.GetHashCode() : 0));

                return (int)(hashCode / 3);
            }
        }

        public override string ToString()
        {
            string version = $"{Major}.{Minor}.{Patch}";

            if (IsPrereleaseIncluded)
                version += $"-{Prerelease}";

            if (IsMetadataIncluded)
                version += $"+{Metadata}";

            return version;
        }

        public static bool Equals(SemVer2 left, SemVer2 right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as SemVer2);
        }
        public bool Equals(SemVer2 obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Major == obj.Major
                   && Minor == obj.Minor
                   && Patch == obj.Patch
                   && string.Equals(Prerelease, obj.Prerelease, StringComparison.Ordinal)
                   && string.Equals(Metadata, obj.Metadata, StringComparison.Ordinal);
        }

        public static int Compare(SemVer2 left, SemVer2 right)
        {
            if (ReferenceEquals(left, right))
                return 0;

            if (left is null)
                return -1;

            if (right is null)
                return 1;

            return left.CompareTo(right);
        }
        public int CompareTo(object obj)
        {
            return CompareTo((SemVer2)obj);
        }
        public int CompareTo(SemVer2 other)
        {
            return CompareTo(other, true);
        }
        public int CompareTo(SemVer2 other, bool includePrerelease)
        {
            if (other is null)
                return 1;

            if (Major != other.Major)
            {
                if (Major > other.Major)
                    return 1;

                return -1;
            }

            if (Minor != other.Minor)
            {
                if (Minor > other.Minor)
                    return 1;

                return -1;
            }

            if (Patch != other.Patch)
            {
                if (Patch > other.Patch)
                    return 1;

                return -1;
            }

            if (includePrerelease && !string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal))
            {
                if (!IsPrereleaseIncluded && !other.IsPrereleaseIncluded)
                    return 0;

                if (!IsPrereleaseIncluded)
                    return 1;

                if (!other.IsPrereleaseIncluded)
                    return -1;

                if (Patch != other.Patch)
                {
                    if (Patch > other.Patch)
                        return 1;

                    return -1;
                }

                int prereleasePartsArrayLength = _prereleasePartsArray.Length;
                int otherPrereleasePartsArrayLength = other._prereleasePartsArray.Length;
                int minLength = Math.Min(prereleasePartsArrayLength, otherPrereleasePartsArrayLength);

                for (int i = 0; i < minLength; ++i)
                {
                    ref string prereleasePart = ref _prereleasePartsArray[i];
                    ref string otherPrereleasePart = ref other._prereleasePartsArray[i];

                    bool prereleasePartIsNumber = uint.TryParse(prereleasePart, out uint prereleasePartNumber);
                    bool otherPrereleasePartIsNumber = uint.TryParse(otherPrereleasePart, out uint otherPrereleasePartNumber);
                    int compare;

                    if (prereleasePartIsNumber && otherPrereleasePartIsNumber)
                    {
                        compare = prereleasePartNumber.CompareTo(otherPrereleasePartNumber);

                        if (compare != 0)
                            return compare;
                    }
                    else if (prereleasePartIsNumber)
                    {
                        return -1;
                    }
                    else if (otherPrereleasePartIsNumber)
                    {
                        return 1;
                    }
                    else
                    {
                        compare = string.CompareOrdinal(prereleasePart, otherPrereleasePart);

                        if (compare != 0)
                            return compare;
                    }
                }

                return prereleasePartsArrayLength.CompareTo(otherPrereleasePartsArrayLength);
            }

            return 0;
        }

        public static bool operator ==(SemVer2 left, SemVer2 right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }
        public static bool operator !=(SemVer2 left, SemVer2 right)
        {
            return !(left == right);
        }

        public static bool operator <(SemVer2 left, SemVer2 right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }
        public static bool operator <=(SemVer2 left, SemVer2 right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(SemVer2 left, SemVer2 right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }
        public static bool operator >=(SemVer2 left, SemVer2 right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
