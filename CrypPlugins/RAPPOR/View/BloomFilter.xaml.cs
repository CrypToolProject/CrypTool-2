using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrypTool.Plugins.RAPPOR.View
{
    /// <summary>
    /// Interaction logic for BloomFilter.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.RAPPOR.Properties.Resources")]
    public partial class BloomFilter : UserControl
    {
        private bool pause;
        /// <summary>
        /// Initializes the Bloom Filter View.
        /// </summary>
        public BloomFilter()
        {
            pause = true;
            InitializeComponent();
            PauseButton.Content = CrypTool.Plugins.RAPPOR.Properties.Resources.PauseAnimation;
            //This might lead to a crash if not probably initialized
            slValueChanged();
            //DataContex = new BloomFilterViewModel();
            //CrypTool.Plugins.RAPPOR.RAPPOR
        }
        /// <summary>
        /// This method is used to validate the input of the textboxes, insuring that only strings 
        /// are entered which can be transformed into integers.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        /// <summary>
        /// This method is used to validate the input of the timerinput, insuring that only strings 
        /// are entered which can be transformed into integers.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void TimerInputValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+(.)[0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void slValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timerInputTxt.Text = slValue.Value.ToString();
        }
        private void slValueChanged()
        {
            timerInputTxt.Text = slValue.Value.ToString();
        }
        /// <summary>
        /// Increases the size of the inputTimer by zero point one.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void cmdUp_Click_inputTimer(object sender, RoutedEventArgs e)
        {
            timerInputTxt.Text = (double.Parse(timerInputTxt.Text) + 0.1).ToString();
        }
        /// <summary>
        /// Decreases the size of the inputTimer by zero point one. It ensures that the value 
        /// will not undergo 0.1.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void cmdDown_Click_inputTimer(object sender, RoutedEventArgs e)
        {
            if (double.Parse(timerInputTxt.Text) >= 1.2)
            {
                timerInputTxt.Text = (double.Parse(timerInputTxt.Text) - 0.1).ToString();
            }
            else
            {
                timerInputTxt.Text = "1";
            }
        }
        /// <summary>
        /// Methodhusk for the the startbutton option. The start button is handled in the
        /// viewmodel.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
        }
        /// <summary>
        /// Methodhust for the reset button option. The reset button is handled in the viewmodel.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (pause)
            {
                PauseButton.Content = CrypTool.Plugins.RAPPOR.Properties.Resources.ResumeAnimation;
                pause = false;
            }
            else
            {
                PauseButton.Content = CrypTool.Plugins.RAPPOR.Properties.Resources.PauseAnimation;
                pause = true;
            }
        }
        private void PreviousStep_Click(object sender, RoutedEventArgs e)
        {
            if (pause)
            {
                PauseButton.Content = CrypTool.Plugins.RAPPOR.Properties.Resources.ResumeAnimation;
                pause = false;
            }
        }
        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Activate()
        {
            StartButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            PreviousStepButton.IsEnabled = true;
            NextStepButton.IsEnabled = true;
        }
        private void Deactivate() 
        {
            StartButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            PreviousStepButton.IsEnabled = false;
            NextStepButton.IsEnabled = false;
        }

    }
}