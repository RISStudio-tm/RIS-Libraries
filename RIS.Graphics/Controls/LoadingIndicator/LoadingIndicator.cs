// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;
using System.Windows.Controls;
using RIS.Graphics.Extensions;

namespace RIS.Graphics.Controls
{
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    public class LoadingIndicator : Control
    {
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(LoadingIndicatorMode), typeof(LoadingIndicator),
                new PropertyMetadata(LoadingIndicatorMode.Arcs));
        public static readonly DependencyProperty SpeedRatioProperty =
            DependencyProperty.Register(nameof(SpeedRatio), typeof(double), typeof(LoadingIndicator),
                new PropertyMetadata(1.0, OnSpeedRatioChangedCallback));
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LoadingIndicator),
                new PropertyMetadata(true, OnIsActiveChangedCallback));



        // ReSharper disable InconsistentNaming

        protected Border PART_Border;

        // ReSharper restore InconsistentNaming



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



        private static void OnSpeedRatioChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as LoadingIndicator;

            target?.OnSpeedRatioChanged(e);
        }

        private static void OnIsActiveChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as LoadingIndicator;

            target?.OnIsActiveChanged(e);
        }



        private void OnSpeedRatioChanged(
            DependencyPropertyChangedEventArgs e)
        {
            if (PART_Border == null || !IsActive)
                return;

            SetStoryBoardSpeedRatio(
                PART_Border, (double)e.NewValue);
        }

        private void OnIsActiveChanged(
            DependencyPropertyChangedEventArgs e)
        {
            if (PART_Border == null)
                return;

            if (!(bool)e.NewValue)
            {
                VisualStateManager.GoToElementState(
                    PART_Border,
                    "Inactive",
                    false);

                PART_Border.SetCurrentValue(
                    VisibilityProperty,
                    Visibility.Collapsed);
            }
            else
            {
                VisualStateManager.GoToElementState(
                    PART_Border,
                    "Active",
                    false);

                PART_Border.SetCurrentValue(
                    VisibilityProperty,
                    Visibility.Visible);

                SetStoryBoardSpeedRatio(
                    PART_Border, SpeedRatio);
            }
        }



        private static void SetStoryBoardSpeedRatio(
            FrameworkElement element, double speedRatio)
        {
            foreach (var activeState in element
                         .GetVisualStateGroupsByName("ActiveStates")
                         .GetAllVisualStatesByName("Active"))
            {
                activeState
                    .Storyboard
                    .SetSpeedRatio(
                        element, speedRatio);
            }
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_Border = (Border)GetTemplateChild(
                nameof(PART_Border));

            if (PART_Border == null)
                return;

            VisualStateManager.GoToElementState(
                PART_Border,
                IsActive
                    ? "Active"
                    : "Inactive", false);

            SetStoryBoardSpeedRatio(
                PART_Border, SpeedRatio);

            PART_Border.SetCurrentValue(
                VisibilityProperty,
                IsActive
                    ? Visibility.Visible
                    : Visibility.Collapsed);
        }
    }
}