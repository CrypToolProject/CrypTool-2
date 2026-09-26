/*
   Copyright (C) CrypTool 2 Team

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
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.Plugins.PaddingOracle
{
    /// <summary>
    /// Interaction logic for OraclePresentation.xaml
    /// </summary>
    public partial class OraclePresentation : UserControl
    {
        public OraclePresentation()
        {
            InitializeComponent();
            Height = 130;
            Width = 293;
            padPointer.Visibility = Visibility.Hidden;

            viewByteScroller.Value = 1;
        }

        public void showPaddingImg(bool valid)
        {
            if (valid)
            {
                padValid.Visibility = Visibility.Visible;
                padInvalid.Visibility = Visibility.Hidden;
            }
            else
            {
                padValid.Visibility = Visibility.Hidden;
                padInvalid.Visibility = Visibility.Visible;
            }
        }

        public void setPadPointer(int padLen, int viewMode)
        {
            if (viewMode == 4)
            {
                padPointer.Width = 3;
                padPointer.BorderThickness = new System.Windows.Thickness(3);
            }
            else
            {
                switch (viewMode)
                {
                    case 0: padPointer.BorderThickness = new System.Windows.Thickness(3); break;
                    case 1: padPointer.BorderThickness = new System.Windows.Thickness(0, 3, 3, 3); break;
                    case 2: padPointer.BorderThickness = new System.Windows.Thickness(0, 3, 0, 3); break;
                    case 3: padPointer.BorderThickness = new System.Windows.Thickness(3, 3, 0, 3); break;
                }

                padPointer.Width = -1 + 29 * padLen;
            }

            padPointer.Margin = new System.Windows.Thickness(270 - 29 * padLen, padPointer.Margin.Top, 0, 0);
            padPointer.Visibility = Visibility.Visible;
        }
    }
}