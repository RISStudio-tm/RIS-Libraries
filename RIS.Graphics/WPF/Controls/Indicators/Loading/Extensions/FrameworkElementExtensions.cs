// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions.Markup;

namespace RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions
{
    internal static class FrameworkElementExtensions
    {
        public static IEnumerable<VisualStateGroup> GetActiveVisualStateGroups(
            this FrameworkElement element)
        {
            return element
                .GetVisualStateGroupsByName(IndicatorVisualStateGroupNames.ActiveStates.Name);
        }

        public static IEnumerable<VisualState> GetActiveVisualStates(
            this FrameworkElement element)
        {
            return element
                .GetActiveVisualStateGroups()
                .GetAllVisualStatesByName(IndicatorVisualStateNames.ActiveState.Name);
        }

        public static IEnumerable<VisualStateGroup> GetVisualStateGroupsByName(
            this FrameworkElement element, string name)
        {
            var groups = VisualStateManager.GetVisualStateGroups(element);

            if (groups is null)
                return null;

            IEnumerable<VisualStateGroup> castedVisualStateGroups;

            try
            {
                castedVisualStateGroups = groups.Cast<VisualStateGroup>().ToArray();

                if (!castedVisualStateGroups.Any())
                    return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(name)
                ? castedVisualStateGroups
                : castedVisualStateGroups.Where(vsg => vsg.Name == name);
        }
    }
}
