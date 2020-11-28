// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;
using System.Windows.Input;

namespace RIS.Graphics.WPF.Windows
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

            Result = MessageBoxResult.None;
        }
        public MessageBoxWindow(MaterialMessageBoxButtons buttons)
        {
            InitializeComponent();

            switch (buttons)
            {
                case MaterialMessageBoxButtons.OK:
                    Result = MessageBoxResult.OK;
                    BtnCancel.Visibility = Visibility.Collapsed;
                    BtnOk.Focus();
                    break;
                case MaterialMessageBoxButtons.OKCancel:
                    Result = MessageBoxResult.Cancel;
                    BtnCancel.Focus();
                    break;
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение MaterialMessageBoxButtons для создания окна MaterialMessageBox", nameof(buttons));
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
            }
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        private void BtnOk_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && BtnOk.IsEnabled)
                BtnOk_OnClick(this, new RoutedEventArgs());
        }
        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void BtnCancel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && BtnCancel.IsEnabled)
                BtnCancel_OnClick(this, new RoutedEventArgs());
        }
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void BtnCopyMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnCopyMessage_OnClick(this, new RoutedEventArgs());
        }
        private void BtnCopyMessage_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TxtMessage.Text);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
