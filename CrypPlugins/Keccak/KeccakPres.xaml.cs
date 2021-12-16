using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.Plugins.Keccak
{
    /// <summary>
    /// Interaktionslogik für KeccakPres.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Keccak.Properties.Resources")]
    public partial class KeccakPres : UserControl
    {
        public AutoResetEvent buttonNextClickedEvent;
        public bool autostep, skipPermutation, skipStep, skipPresentation, skipIntro;
        public int autostepSpeed;

        public KeccakPres()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            skipStep = false;
            autostep = false;
            skipPresentation = false;
            skipPermutation = false;
            skipIntro = false;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            buttonNextClickedEvent.Set();
        }

        private void buttonSkipIntro_Click(object sender, RoutedEventArgs e)
        {
            buttonNextClickedEvent.Set();
            skipIntro = true;
        }

        private void buttonAutostep_Click(object sender, RoutedEventArgs e)
        {
            autostep = !autostep;
            if (autostep)
            {
                buttonNextClickedEvent.Set();
            }
        }

        private void buttonSkip_Click(object sender, RoutedEventArgs e)
        {
            skipStep = true;
            if (!autostep)
            {
                buttonNextClickedEvent.Set();
            }
        }

        private void buttonSkipPermutation_Click(object sender, RoutedEventArgs e)
        {
            skipPermutation = true;
            if (!autostep)
            {
                buttonNextClickedEvent.Set();
            }
        }

        private void autostepSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            autostepSpeed = 10 + 10 * (40 - (int)autostepSpeedSlider.Value);
        }

    }
}
