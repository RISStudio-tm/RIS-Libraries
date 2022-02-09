// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RIS.Graphics.Controls
{
    public class ExtendedScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty SpeedRatioProperty =
            DependencyProperty.Register(nameof(SpeedRatio), typeof(double), typeof(ExtendedScrollViewer),
                new PropertyMetadata(1.0));



        public double SpeedRatio
        {
            get
            {
                return (double)GetValue(SpeedRatioProperty);
            }
            set
            {
                SetValue(SpeedRatioProperty, value);
            }
        }



        protected override void OnPreviewMouseWheel(
            MouseWheelEventArgs e)
        {
            if (e.Handled
                || !(ScrollInfo is ScrollContentPresenter scrollContent))
            {
                return;
            }

            if (ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                var offset = VerticalOffset - (e.Delta * SpeedRatio);

                if (offset < 0)
                {
                    scrollContent.SetVerticalOffset(0);
                }
                else if (offset > ExtentHeight)
                {
                    scrollContent.SetVerticalOffset(ExtentHeight);
                }
                else
                {
                    scrollContent.SetVerticalOffset(offset);
                }
            }
            else if (ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                var offset = HorizontalOffset - (e.Delta * SpeedRatio);

                if (offset < 0)
                {
                    scrollContent.SetHorizontalOffset(0);
                }
                else if (offset > ExtentWidth)
                {
                    scrollContent.SetHorizontalOffset(ExtentWidth);
                }
                else
                {
                    scrollContent.SetHorizontalOffset(offset);
                }
            }

            e.Handled = true;
        }
    };
}
