// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions
{
    internal static class LoadingIndicatorModeExtensions
    {
        public static string GetStyleFileName(this LoadingIndicatorMode value)
        {
            return value.GetDescription();
        }

        public static string GetStyleName(this LoadingIndicatorMode value)
        {
            return value.GetDescription() + "Style";
        }

        public static string GetStyleKey(this LoadingIndicatorMode value)
        {
            return value.GetDescription() + "Key";
        }
    }
}
