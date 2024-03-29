﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace RIS.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            return value
                .GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;
        }



        public static T GetDefaultValue<T>(this Enum target)
            where T : struct, Enum
        {
            return (T)GetDefaultValue(target);
        }

        public static Enum GetDefaultValue(this Enum target)
        {
            return target
                .GetType()
                .GetEnumDefaultValue();
        }
    }
}
