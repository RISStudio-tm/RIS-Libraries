// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Collections.Nestable.Serialization
{
    [Flags]
    public enum NestableSerializerTargets : uint
    {
        None = 0,
        PublicProperties = 1,
        NonPublicProperties = 2,
        PublicFields = 4,
        NonPublicFields = 8,
        Default = PublicProperties,
        AnyPublic = PublicProperties
                    | PublicFields,
        AnyProperties = PublicProperties
                        | NonPublicProperties,
        Any = PublicProperties
              | NonPublicProperties
              | PublicFields
              | NonPublicFields
    }

    public enum NestableSerializerCollectionType : byte
    {
        NestableDictionaryL = 1,
        Default = NestableDictionaryL
    }
}
