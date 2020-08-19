// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using RIS.Extensions;

namespace RIS.Versioning
{
    public sealed class PartialSemVer2
    {
        public static readonly string[] AnyNumberChars = { "X", "x", "*" };

        public static readonly Regex VersionInfoAllowZerosVersionRegex = new Regex(@"^(?:[v][\.]?)?(?<major>(?:[xX\*]|[0]|[1-9][0-9]*))(?:[\.](?<minor>(?:[xX\*]|[0]|[1-9][0-9]*)))?(?:[\.](?<patch>(?:[xX\*]|[0]|[1-9][0-9]*)))?(?:[-]?(?(?<=[-])(?<prerelease>(?:(?<prerelease_part>(?(?=[0](?:[\.]|[\+]|$))[0]|[A-Za-z1-9][A-Za-z0-9]*))(?(?=(?:[\.]$|[\.][\+]))(?!)|[\.])?)+)|(?:$|[\+])))(?:[\+]?(?(?<=[+])(?<metadata>(?:(?<metadata_part>[A-Za-z0-9]+)(?(?=[\.]$)(?!)|[\.])?)+)|$))$", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
        public static readonly Regex VersionInfoRegex = new Regex(@"^(?:[v][\.]?)?(?<major>(?:[xX\*]|[0]|[1-9][0-9]*))(?:[\.](?<minor>(?:[xX\*]|[0]|[1-9][0-9]*)))?(?:[\.](?<patch>(?(?<=(?:[1-9xX\*][\.]|[1-9xX\*][\.](?:[xX\*]|[0-9]+)[\.]))(?:[xX\*]|[0]|[1-9][0-9]*)|(?:[xX\*]|[1-9][0-9]*))))?(?:[-]?(?(?<=[-])(?<prerelease>(?:(?<prerelease_part>(?(?=[0](?:[\.]|[\+]|$))[0]|[A-Za-z1-9][A-Za-z0-9]*))(?(?=(?:[\.]$|[\.][\+]))(?!)|[\.])?)+)|(?:$|[\+])))(?:[\+]?(?(?<=[+])(?<metadata>(?:(?<metadata_part>[A-Za-z0-9]+)(?(?=[\.]$)(?!)|[\.])?)+)|$))$", RegexOptions.Multiline, TimeSpan.FromSeconds(5));

        public bool IsAnyMajor { get; }
        public uint? Major { get; }
        public bool IsAnyMinor { get; }
        public uint? Minor { get; }
        public bool IsAnyPatch { get; }
        public uint? Patch { get; }
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
        public bool IsFullRelease
        {
            get
            {
                return Major.HasValue && Minor.HasValue && Patch.HasValue;
            }
        }
        public bool IsFullPrerelease
        {
            get
            {
                return Major.HasValue && Minor.HasValue && Patch.HasValue
                       && IsPrereleaseIncluded;
            }
        }
        public bool IsFull
        {
            get
            {
                return Major.HasValue && Minor.HasValue && Patch.HasValue
                       && IsPrereleaseIncluded && IsMetadataIncluded;
            }
        }

        public PartialSemVer2(string version, bool allowZerosVersion = false)
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
                    ? new FormatException($"Не удалось распознать формат Semanic Version 2.0.0 (с поддержкой нулевой версии и с поддержкой wildcards) в строке [{version}]")
                    : new FormatException($"Не удалось распознать формат Semanic Version 2.0.0 (без поддержки нулевой версии и с поддержкой wildcards) в строке [{version}]");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (AnyNumberChars.Contains(match.Groups["major"].Value))
            {
                Major = null;
                IsAnyMajor = true;
            }
            else
            {
                Major = match.Groups["major"].Value.ToUInt();
            }

            if (match.Groups["minor"].Success)
            {
                if (AnyNumberChars.Contains(match.Groups["minor"].Value))
                {
                    Minor = null;
                    IsAnyMinor = true;
                }
                else
                {
                    Minor = match.Groups["minor"].Value.ToUInt();
                }
            }

            if (match.Groups["patch"].Success)
            {
                if (AnyNumberChars.Contains(match.Groups["patch"].Value))
                {
                    Patch = null;
                    IsAnyPatch = true;
                }
                else
                {
                    Patch = match.Groups["patch"].Value.ToUInt();
                }
            }

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

        public SemVer2 ToSemVer2(bool allowZerosVersion = false)
        {
            return new SemVer2(Major ?? 0, Minor ?? 0, Patch ?? 0, Prerelease, Metadata, allowZerosVersion);
        }
    }
}
