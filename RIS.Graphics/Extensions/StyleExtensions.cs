﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;

namespace RIS.Graphics.Extensions
{
    public static class StyleExtensions
    {
        public static void Merge(
            this Style style1, Style style2)
        {
            if (style1 == null)
                throw new ArgumentNullException(nameof(style1));
            if (style2 == null)
                throw new ArgumentNullException(nameof(style2));

            if (style1.TargetType.IsAssignableFrom(style2.TargetType))
                style1.TargetType = style2.TargetType;

            if (style2.BasedOn != null)
                Merge(style1, style2.BasedOn);

            foreach (var setter in style2.Setters)
            {
                style1.Setters.Add(
                    setter);
            }

            foreach (var trigger in style2.Triggers)
            {
                style1.Triggers.Add(
                    trigger);
            }

            foreach (var key in style2.Resources.Keys)
            {
                style1.Resources[key] =
                    style2.Resources[key];
            }
        }
    }
}
