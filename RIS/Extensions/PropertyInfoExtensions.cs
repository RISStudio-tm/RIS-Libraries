// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Reflection;

namespace RIS.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static PropertyInfo GetDeclaring(this PropertyInfo propertyInfo)
        {
            try
            {
                return propertyInfo.DeclaringType?.GetProperty(propertyInfo.Name)
                       ?? propertyInfo;
            }
            catch (Exception)
            {
                return propertyInfo;
            }
        }
    }
}
