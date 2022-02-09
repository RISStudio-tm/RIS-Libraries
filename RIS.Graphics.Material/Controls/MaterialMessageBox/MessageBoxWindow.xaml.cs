// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;
using System.Windows.Input;

namespace RIS.Graphics.Material.Controls
{
    public partial class MessageBoxWindow: IDisposable
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;



        public MessageBoxResult Result { get; protected set; }



        public MessageBoxWindow()
        {
            InitializeComponent();
            DataContext = this;

            Result = MessageBoxResult.None;
        }
        public MessageBoxWindow(
            MaterialMessageBoxButtons buttons)
        {
            InitializeComponent();
            DataContext = this;

            switch (buttons)
            {
                case MaterialMessageBoxButtons.OK:
                    Result = MessageBoxResult.OK;
                    CancelButton.Visibility = Visibility.Collapsed;

                    OkButton.Focus();
                    break;
                case MaterialMessageBoxButtons.OKCancel:
                    Result = MessageBoxResult.Cancel;

                    CancelButton.Focus();
                    break;
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение MaterialMessageBoxButtons для создания окна MaterialMessageBox", nameof(buttons));
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }



        public void OnInformation(
            RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender,
            RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(
            RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender,
            RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(
            RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender,
            RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }



        public void Dispose()
        {
            Close();
        }



        private void CopyMessageButton_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CopyMessageButton_OnClick(this,
                    new RoutedEventArgs());
            }
        }
        private void CopyMessageButton_OnClick(object sender,
            RoutedEventArgs e)
        {
            Clipboard.SetText(
                MessageTextBox.Text);
        }

        private void OkButton_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter
                && OkButton.IsEnabled)
            {
                OkButton_OnClick(this,
                    new RoutedEventArgs());
            }
        }
        private void OkButton_OnClick(object sender,
            RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;

            Close();
        }

        private void CancelButton_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter
                && CancelButton.IsEnabled)
            {
                CancelButton_OnClick(this,
                    new RoutedEventArgs());
            }
        }
        private void CancelButton_OnClick(object sender,
            RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;

            Close();
        }
    }
}
