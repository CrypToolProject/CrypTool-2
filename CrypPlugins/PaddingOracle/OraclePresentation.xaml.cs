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