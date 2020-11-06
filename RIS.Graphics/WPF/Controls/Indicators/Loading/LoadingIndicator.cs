// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;
using System.Windows.Controls;
using RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions;
using RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions.Markup;

namespace RIS.Graphics.WPF.Controls.Indicators.Loading
{
    [TemplatePart(Name = TemplateBorderName, Type = typeof(Border))]
    public class LoadingIndicator : Control
    {
        public static readonly DependencyProperty SpeedRatioProperty =
            DependencyProperty.Register("SpeedRatio", typeof(double), typeof(LoadingIndicator),
                new PropertyMetadata(1d, OnSpeedRatioChanged));
        // ReSharper disable once InconsistentNaming
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(LoadingIndicator),
                new PropertyMetadata(true, OnIsActiveChanged));
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(LoadingIndicatorMode), typeof(LoadingIndicator),
                new PropertyMetadata(default(LoadingIndicatorMode)));

        internal const string TemplateBorderName = "PART_Border";

        // ReSharper disable once InconsistentNaming
        protected Border PART_Border;

        public LoadingIndicatorMode Mode
        {
            get
            {
                return (LoadingIndicatorMode)GetValue(ModeProperty);
            }
            set
            {
                SetValue(ModeProperty, value);
            }
        }
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
        public bool IsActive
        {
            get
            {
                return (bool)GetValue(IsActiveProperty);
            }
            set
            {
                SetValue(IsActiveProperty, value);
            }
        }

        static LoadingIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingIndicator),
                new FrameworkPropertyMetadata(typeof(LoadingIndicator)));
        }

        private static void SetStoryBoardSpeedRatio(FrameworkElement element, double speedRatio)
        {
            foreach (var activeState in element.GetActiveVisualStates())
            {
                activeState.Storyboard.SetSpeedRatio(element, speedRatio);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_Border = (Border) GetTemplateChild(TemplateBorderName);

            if (PART_Border == null)
                return;

            VisualStateManager.GoToElementState(PART_Border,
                IsActive
                    ? IndicatorVisualStateNames.ActiveState.Name
                    : IndicatorVisualStateNames.InactiveState.Name, false);

            SetStoryBoardSpeedRatio(PART_Border, SpeedRatio);

            PART_Border.SetCurrentValue(VisibilityProperty,
                IsActive
                    ? Visibility.Visible
                    : Visibility.Collapsed);
        }

        private static void OnSpeedRatioChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var indicator = (LoadingIndicator)obj;

            if (indicator.PART_Border == null || !indicator.IsActive)
                return;

            SetStoryBoardSpeedRatio(indicator.PART_Border, (double)e.NewValue);
        }

        private static void OnIsActiveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var indicator = (LoadingIndicator)obj;

            if (indicator.PART_Border == null)
                return;

            if (!(bool)e.NewValue)
            {
                VisualStateManager.GoToElementState(indicator.PART_Border,
                    IndicatorVisualStateNames.InactiveState.Name,
                    false);

                indicator.PART_Border.SetCurrentValue(VisibilityProperty,
                    Visibility.Collapsed);
            }
            else
            {
                VisualStateManager.GoToElementState(indicator.PART_Border,
                    IndicatorVisualStateNames.ActiveState.Name, false);

                indicator.PART_Border.SetCurrentValue(VisibilityProperty,
                    Visibility.Visible);

                SetStoryBoardSpeedRatio(indicator.PART_Border, indicator.SpeedRatio);
            }
        }
    }
}