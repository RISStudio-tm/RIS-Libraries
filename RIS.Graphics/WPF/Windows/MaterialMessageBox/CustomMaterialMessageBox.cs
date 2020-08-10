// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

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
