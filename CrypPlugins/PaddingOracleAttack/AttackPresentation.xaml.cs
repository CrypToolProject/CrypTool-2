using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.Plugins.PaddingOracleAttack
{
    /// <summary>
    /// Interaction logic for AttackPresentation.xaml
    /// </summary>

    [CrypTool.PluginBase.Attributes.Localization("PaddingOracleAttack.Properties.Resources")]
    public partial class AttackPresentation : UserControl
    {
        public AttackPresentation()
        {
            InitializeComponent();
            Width = 582;
            Height = 406;

            imgPhase[0] = phase1;
            imgPhase[1] = phase2;
            imgPhase[2] = phase3;

            this.viewByteScroller.Value = 1;
            //this.viewByteScroller.Minimum = 0.4;
        }

        public void padInput(bool valid)
        {
            //if valid, then show the "valid" image. if not, show the "invalid" image
            if (valid)
            {
                this.inPadValid.Visibility = System.Windows.Visibility.Visible;
                this.inPadInvalid.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                this.inPadValid.Visibility = System.Windows.Visibility.Hidden;
                this.inPadInvalid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void setBytePointer(int position, bool visible)
        {
            System.Windows.Visibility vis = System.Windows.Visibility.Hidden;
            if (visible)
            {
                vis = System.Windows.Visibility.Visible;
            }

            int pos;

            if (position < 0)
            {
                this.bytePointer.Width = 3;
                pos = 0;
            }
            else if (position > 7)
            {
                this.bytePointer.Width = 3;
                pos = 8;
            }
            else
            {
                this.bytePointer.Width = 24;
                pos = position;
            }

            this.bytePointer.Visibility = vis;
            this.bytePointer.Margin = new System.Windows.Thickness(attDecBlock.Margin.Left - 2 + (230 * pos) / 8, bytePointer.Margin.Top, 0, 0);
        }

        private readonly Image[] imgPhase = new Image[3];
        public void setPhase(int phaseNum)
        {
            for (int imgCounter = 0; imgCounter < 3; imgCounter++)
            {
                imgPhase[imgCounter].Visibility = Visibility.Hidden;
            }

            imgPhase[phaseNum - 1].Visibility = Visibility.Visible;
        }

        public void changeBorderColor(bool endOfPhase)
        {
            if (endOfPhase)
            {
                this.descBorder.BorderBrush = Brushes.Red;
            }
            else
            {
                this.descBorder.BorderBrush = Brushes.Gray;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        /*
        private void Test_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            this.descShownBytes.Text = e.ToString() + ", " + sender.ToString();
            double value = this.viewByteScroller.Value;
            this.descShownBytes.Text = String.Format("{0:0.00}", value);
        }
        */


    }
}
