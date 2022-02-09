// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;
using System.Windows.Media;

namespace RIS.Graphics.Material.Controls
{
    public static class MaterialMessageBox
    {
        public static MessageBoxResult ShowInfo(
            string message,
            string title = "Information",
            MaterialMessageBoxButtons buttons = MaterialMessageBoxButtons.OK,
            bool isRightToLeft = false)
        {
            using var msg = new MessageBoxWindow(
                buttons);

            msg.Title = title;
            msg.TitleTextBox.Text = title;
            msg.MessageTextBox.Text = message;
            msg.TitleBackgroundPanel.Background =
                new SolidColorBrush(
                    Color.FromRgb(3, 169, 244));
            msg.BorderBrush =
                new SolidColorBrush(
                    Color.FromRgb(3, 169, 244));

            if (isRightToLeft)
                msg.FlowDirection = FlowDirection.RightToLeft;

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowWarning(
            string message,
            string title = "Warning",
            MaterialMessageBoxButtons buttons = MaterialMessageBoxButtons.OK,
            bool isRightToLeft = false)
        {
            using var msg = new MessageBoxWindow(
                buttons);

            msg.Title = title;
            msg.TitleTextBox.Text = title;
            msg.MessageTextBox.Text = message;
            msg.TitleBackgroundPanel.Background =
                Brushes.Orange;
            msg.BorderBrush =
                Brushes.Orange;

            if (isRightToLeft)
                msg.FlowDirection = FlowDirection.RightToLeft;

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowError(
            string message,
            string title = "Error",
            MaterialMessageBoxButtons buttons = MaterialMessageBoxButtons.OK,
            bool isRightToLeft = false)
        {
            using var msg = new MessageBoxWindow(
                buttons);

            msg.Title = title;
            msg.TitleTextBox.Text = title;
            msg.MessageTextBox.Text = message;
            msg.TitleBackgroundPanel.Background =
                Brushes.Red;
            msg.BorderBrush =
                Brushes.Red;

            if (isRightToLeft)
                msg.FlowDirection = FlowDirection.RightToLeft;

            msg.ShowDialog();

            return msg.Result;
        }
    }
}
