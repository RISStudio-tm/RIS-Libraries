// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RIS.Graphics.WPF.Controls
{
    internal sealed class ExtendedPasswordBox : TextBox
    {
        public static readonly DependencyProperty PasswordProperty =
          DependencyProperty.Register("Password", typeof(SecureString), typeof(ExtendedPasswordBox), new UIPropertyMetadata(new SecureString()));
        public static readonly DependencyProperty HiddenTextProperty =
          DependencyProperty.Register("HiddenText", typeof(string), typeof(ExtendedPasswordBox), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty PasswordVisibleProperty =
          DependencyProperty.Register("PasswordVisible", typeof(bool), typeof(ExtendedPasswordBox), new UIPropertyMetadata(false));

        private const char PasswordChar = '●';

        private readonly DispatcherTimer _maskTimer;

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

                PasswordVisibilityChanged();
            }
        }

        public ExtendedPasswordBox()
        {
            PreviewTextInput += OnPreviewTextInput;
            PreviewKeyDown += OnPreviewKeyDown;
            TextChanged += OnTextChanged;

            CommandManager.AddPreviewExecutedHandler(this, PreviewExecutedHandler);

            _maskTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0) };
            _maskTimer.Tick += (sender, args) => MaskAllDisplayText();
        }

        private void AddToSecureString(string text)
        {
            if (SelectionLength > 0)
                RemoveFromSecureString(SelectionStart, SelectionLength);

            foreach (char c in text)
            {
                int caretIndex = CaretIndex;

                Password.InsertAt(caretIndex, c);

                HiddenText = HiddenText.Insert(caretIndex, c.ToString());

                MaskAllDisplayText();

                if (caretIndex == Text.Length)
                {
                    _maskTimer.Stop();
                    _maskTimer.Start();

                    Text = Text.Insert(caretIndex++, c.ToString());
                }
                else
                {
                    Text = Text.Insert(caretIndex++, PasswordChar.ToString());
                }

                CaretIndex = caretIndex;
            }
        }

        private void RemoveFromSecureString(int startIndex, int trimLength)
        {
            int caretIndex = CaretIndex;

            for (int i = 0; i < trimLength; ++i)
            {
                Password.RemoveAt(startIndex);

                HiddenText = HiddenText.Remove(startIndex, 1);
            }

            Text = Text.Remove(startIndex, trimLength);
            CaretIndex = caretIndex;
        }

        private void MaskAllDisplayText()
        {
            _maskTimer.Stop();

            int caretIndex = CaretIndex;
            Text = new string(PasswordChar, Text.Length);
            CaretIndex = caretIndex;
        }

        private void PasswordVisibilityChanged()
        {
            //PasswordVisible = !PasswordVisible;
            int caretIndex = CaretIndex;

            Text = PasswordVisible ? HiddenText : string.Empty;

            if (!PasswordVisible)
            {
                for (int i = 0; i < Password.Length; i++)
                {
                    Text += PasswordChar;
                }
            }

            Focus();

            CaretIndex = caretIndex;
        }

        private void PreviewExecutedHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (executedRoutedEventArgs.Command == ApplicationCommands.Copy ||
                executedRoutedEventArgs.Command == ApplicationCommands.Cut ||
                executedRoutedEventArgs.Command == ApplicationCommands.Paste)
            {
                executedRoutedEventArgs.Handled = true;
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            AddToSecureString(textCompositionEventArgs.Text);

            textCompositionEventArgs.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            Key pressedKey = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;

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
                        RemoveFromSecureString(SelectionStart, SelectionLength);
                    }
                    else if (pressedKey == Key.Delete && CaretIndex < Text.Length)
                    {
                        RemoveFromSecureString(CaretIndex, 1);
                    }
                    else if (pressedKey == Key.Back && CaretIndex > 0)
                    {
                        int caretIndex = CaretIndex;

                        if (CaretIndex > 0 && CaretIndex < Text.Length)
                            --caretIndex;

                        RemoveFromSecureString(CaretIndex - 1, 1);

                        CaretIndex = caretIndex;
                    }
                    else
                    {
                        keyEventArgs.Handled = true;
                    }
                    break;
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (PasswordVisible)
                Text = HiddenText;
        }
    }
}
