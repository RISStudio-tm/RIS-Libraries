using System;
using System.Windows;
using System.Windows.Input;

namespace RIS.Graphics.WPF.Windows
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow: IDisposable
    {
        public MessageBoxResult Result { get; set; }

        public MessageBoxWindow()
        {
            InitializeComponent();
            Result = MessageBoxResult.Cancel;

            if (!BtnCancel.IsVisible)
            {
                BtnOk.Focus();
            }
            else
            {
                BtnCancel.Focus();
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void BtnCopyMessage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
               Clipboard.SetText(TxtMessage.Text);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        private void BtnOk_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (BtnOk.IsEnabled)
                {
                    BtnOk_OnClick(null, new RoutedEventArgs());
                }
            }
        }

        private void BtnCancel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (BtnCancel.IsEnabled)
                {
                    BtnCancel_OnClick(null, new RoutedEventArgs());
                }
            }
        }

        private void BtnCopyMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnCopyMessage_OnClick(null, new RoutedEventArgs());
            }
        }
    }
}
