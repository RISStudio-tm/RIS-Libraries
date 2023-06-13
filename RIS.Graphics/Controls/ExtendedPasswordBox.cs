// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RIS.Graphics.Controls
{
    public class ExtendedPasswordBox : TextBox
    {
        public static readonly DependencyProperty PasswordCharProperty =
            DependencyProperty.Register(nameof(PasswordChar), typeof(char), typeof(ExtendedPasswordBox),
                new PropertyMetadata('●'));
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(SecureString), typeof(ExtendedPasswordBox),
                new PropertyMetadata(new SecureString()));
        public static readonly DependencyProperty HiddenTextProperty =
            DependencyProperty.Register(nameof(HiddenText), typeof(string), typeof(ExtendedPasswordBox),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty PasswordVisibleProperty =
            DependencyProperty.Register(nameof(PasswordVisible), typeof(bool), typeof(ExtendedPasswordBox),
                new PropertyMetadata(false, PasswordVisibleChangedCallback));



        private readonly DispatcherTimer _maskTimer;



        public char PasswordChar
        {
            get
            {
                return (char)GetValue(PasswordCharProperty);
            }
            set
            {
                SetValue(PasswordCharProperty, value);
            }
        }
        public SecureString Password
        {
            get
            {
                return (SecureString)GetValue(PasswordProperty);
            }
            set
            {
                SetValue(PasswordProperty, value);
            }
        }
        public string HiddenText
        {
            get
            {
                return (string)GetValue(HiddenTextProperty);
            }
            set
            {
                SetValue(HiddenTextProperty, value);
            }
        }
        public bool PasswordVisible
        {
            get
            {
                return (bool)GetValue(PasswordVisibleProperty);
            }
            set
            {
                SetValue(PasswordVisibleProperty, value);
            }
        }



        public ExtendedPasswordBox()
        {
            PreviewTextInput += OnPreviewTextInput;
            PreviewKeyDown += OnPreviewKeyDown;
            TextChanged += OnTextChanged;

            CommandManager.AddPreviewExecutedHandler(
                this, PreviewExecutedHandler);

            _maskTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(
                    0, 0, 0)
            };
            _maskTimer.Tick += MaskTimer_Tick;
        }



        private static void PasswordVisibleChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as ExtendedPasswordBox;

            target?.OnPasswordVisibleChanged(e);
        }



#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        // ReSharper disable UnusedParameter.Local

        private void OnPasswordVisibleChanged(
            DependencyPropertyChangedEventArgs e)
        {
            var caretIndex = CaretIndex;

            Text = PasswordVisible
                ? HiddenText
                : string.Empty;

            if (!PasswordVisible)
            {
                for (int i = 0; i < Password.Length; ++i)
                {
                    Text += PasswordChar;
                }
            }

            Focus();

            CaretIndex = caretIndex;
        }

        // ReSharper restore UnusedParameter.Local
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр



        private void AddToSecureString(
            string text)
        {
            if (SelectionLength > 0)
            {
                RemoveFromSecureString(
                    SelectionStart, SelectionLength);
            }

            foreach (var ch in text)
            {
                var caretIndex = CaretIndex;

                Password.InsertAt(
                    caretIndex,
                    ch);

                HiddenText = HiddenText.Insert(
                    caretIndex,
                    ch.ToString());

                MaskAllDisplayText();

                if (caretIndex == Text.Length)
                {
                    _maskTimer.Stop();
                    _maskTimer.Start();

                    Text = Text.Insert(
                        caretIndex++,
                        ch.ToString());
                }
                else
                {
                    Text = Text.Insert(
                        caretIndex++,
                        PasswordChar.ToString());
                }

                CaretIndex = caretIndex;
            }
        }

        private void RemoveFromSecureString(
            int startIndex, int length)
        {
            var caretIndex = CaretIndex;

            for (int i = 0; i < length; ++i)
            {
                Password.RemoveAt(
                    startIndex);

                HiddenText = HiddenText.Remove(
                    startIndex, 1);
            }

            Text = Text.Remove(
                startIndex, length);

            CaretIndex = caretIndex;
        }

        private void MaskAllDisplayText()
        {
            _maskTimer.Stop();

            var caretIndex = CaretIndex;

            Text = new string(
                PasswordChar, Text.Length);

            CaretIndex = caretIndex;
        }



        private void OnPreviewTextInput(object sender,
            TextCompositionEventArgs textCompositionEventArgs)
        {
            AddToSecureString(
                textCompositionEventArgs.Text);

            textCompositionEventArgs.Handled = true;
        }

#pragma warning disable SS018 // Add cases for missing enum member.
        // ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            var pressedKey = keyEventArgs.Key == Key.System
                ? keyEventArgs.SystemKey
                : keyEventArgs.Key;

            switch (pressedKey)
            {
                case Key.Space:
                    AddToSecureString(" ");
                    keyEventArgs.Handled = true;

                    break;
                case Key.Back:
                case Key.Delete:
                    if (SelectionLength > 0)
                    {
                        RemoveFromSecureString(
                            SelectionStart, SelectionLength);
                    }
                    else if (pressedKey == Key.Delete && CaretIndex < Text.Length)
                    {
                        RemoveFromSecureString(
                            CaretIndex, 1);
                    }
                    else if (pressedKey == Key.Back && CaretIndex > 0)
                    {
                        var caretIndex = CaretIndex;

                        if (CaretIndex > 0 && CaretIndex < Text.Length)
                            --caretIndex;

                        RemoveFromSecureString(
                            CaretIndex - 1, 1);

                        CaretIndex = caretIndex;
                    }
                    else
                    {
                        keyEventArgs.Handled = true;
                    }

                    break;
                default:
                    break;
            }
        }
        // ReSharper restore SwitchStatementHandlesSomeKnownEnumValuesWithDefault
#pragma warning restore SS018 // Add cases for missing enum member.

        private void OnTextChanged(object sender,
            TextChangedEventArgs textChangedEventArgs)
        {
            if (PasswordVisible)
                Text = HiddenText;
        }



        private void PreviewExecutedHandler(object sender,
            ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (executedRoutedEventArgs.Command == ApplicationCommands.Copy ||
                executedRoutedEventArgs.Command == ApplicationCommands.Cut ||
                executedRoutedEventArgs.Command == ApplicationCommands.Paste)
            {
                executedRoutedEventArgs.Handled = true;
            }
        }



        private void MaskTimer_Tick(object sender,
            EventArgs e)
        {
            MaskAllDisplayText();
        }
    }
}
