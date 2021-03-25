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
            this.Height=130;
            this.Width=293;
            this.padPointer.Visibility = Visibility.Hidden;

            this.viewByteScroller.Value = 1;
        }

        public void showPaddingImg(bool valid) 
        {
            if (valid)
            {
                this.padValid.Visibility = Visibility.Visible;
                this.padInvalid.Visibility = Visibility.Hidden;
            }
            else
            {
                this.padValid.Visibility = Visibility.Hidden;
                this.padInvalid.Visibility = Visibility.Visible;
            }
        }

        public void setPadPointer(int padLen, int viewMode)
        {
            if (viewMode == 4)
            {
                this.padPointer.Width = 3; 
                this.padPointer.BorderThickness = new System.Windows.Thickness(3);
            }
            else
            {
                switch (viewMode)
                {
                    case 0: this.padPointer.BorderThickness = new System.Windows.Thickness(3); break;
                    case 1: this.padPointer.BorderThickness = new System.Windows.Thickness(0, 3, 3, 3); break;
                    case 2: this.padPointer.BorderThickness = new System.Windows.Thickness(0, 3, 0, 3); break;
                    case 3: this.padPointer.BorderThickness = new System.Windows.Thickness(3, 3, 0, 3); break;
                }

                this.padPointer.Width = -1 + 29 * padLen;
            }

            this.padPointer.Margin = new System.Windows.Thickness(270 - 29 * padLen, padPointer.Margin.Top, 0, 0);
            this.padPointer.Visibility = Visibility.Visible;
        } 
    }
}