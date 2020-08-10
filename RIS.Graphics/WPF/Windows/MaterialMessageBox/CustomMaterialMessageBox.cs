using System.Windows;

namespace RIS.Graphics.WPF.Windows
{
    public sealed class CustomMaterialMessageBox : MessageBoxWindow
    {
        public new void Show()
        {
            base.Show();
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }

        public MessageBoxResult ShowDialogWithResult()
        {
            ShowDialog();

            return Result;
        }
    }
}
