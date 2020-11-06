// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions
{
    internal static class VisualStateGroupExtensions
    {
        public static IEnumerable<VisualState> GetAllVisualStatesByName(
            this IEnumerable<VisualStateGroup> visualStateGroups, string name)
        {
            return visualStateGroups
                .SelectMany(vsg => vsg.GetVisualStatesByName(name));
        }

        public static IEnumerable<VisualState> GetVisualStatesByName(
            this VisualStateGroup visualStateGroup, string name)
        {
            if (visualStateGroup is null)
                return null;

            var visualStates = visualStateGroup.GetVisualStates();

            return string.IsNullOrWhiteSpace(name)
                ? visualStates
                : visualStates?.Where(vs => vs.Name == name);
        }

        public static IEnumerable<VisualState> GetVisualStates(
            this VisualStateGroup visualStateGroup)
        {
            if (visualStateGroup is null)
                return null;

            return visualStateGroup.States.Count == 0
                ? null
                : visualStateGroup.States.Cast<VisualState>();
        }
    }
}
