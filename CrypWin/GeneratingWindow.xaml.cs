/*
   Copyright 2008 - 2022 CrypTool Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase.Attributes;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CrypTool.CrypWin
{

    /// <summary>
    /// Interaction logic for GeneratingWindow.xaml
    /// </summary>
    [Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class GeneratingWindow : Window
    {
        private const int GwlStyle = -16;
        private const int WsSysmenu = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public GeneratingWindow()
        {
            InitializeComponent();
            SourceInitialized += GeneratingWindowSourceInitialized;
        }

        /// <summary>
        /// Hide close button of this window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GeneratingWindowSourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            int style = GetWindowLong(wih.Handle, GwlStyle);
            SetWindowLong(wih.Handle, GwlStyle, style & ~WsSysmenu);
        }

        /// <summary>
        /// Sets the message of this window
        /// </summary>
        /// <param name="msg"></param>
        public void SetMessage(string msg)
        {
            Message.Content = msg;
        }
    }
}
