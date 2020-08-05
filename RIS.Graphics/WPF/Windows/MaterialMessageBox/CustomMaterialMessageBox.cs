using System.Windows;

namespace RIS.Graphics.WPF.Windows
{
    public class CustomMaterialMessageBox : MessageBoxWindow
    {
        public new void Show()
        {
            ShowDialog();
        }

        //public MessageBoxResult ShowWithReturnResult()
        //{
        //    ShowDialog();
        //    return Result;
        //}
    }
}
