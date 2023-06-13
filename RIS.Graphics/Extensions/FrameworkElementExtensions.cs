// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RIS.Graphics.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static IEnumerable<VisualStateGroup> GetVisualStateGroupsByName(
            this FrameworkElement element, string name)
        {
            var groups = VisualStateManager
                .GetVisualStateGroups(element);

            if (groups is null)
                return null;

            IEnumerable<VisualStateGroup> resultGroups;

            try
            {
                resultGroups = groups
                    .Cast<VisualStateGroup>()
                    .ToArray();

                if (!resultGroups.Any())
                    return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(name)
                ? resultGroups
                : resultGroups
                    .Where(group =>
                        group.Name == name);
        }
    }
}
