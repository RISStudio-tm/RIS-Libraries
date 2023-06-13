// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Reflection.Mapping
{
    [AttributeUsage(AttributeTargets.Method
                    | AttributeTargets.Property | AttributeTargets.Field
                    | AttributeTargets.Class | AttributeTargets.Struct
                    | AttributeTargets.Interface | AttributeTargets.Enum
                    | AttributeTargets.Delegate | AttributeTargets.Event)]
    public sealed class ExcludedMappingAttribute : Attribute
    {

    }
}
