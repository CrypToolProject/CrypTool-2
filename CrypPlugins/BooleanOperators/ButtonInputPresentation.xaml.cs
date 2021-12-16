using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BooleanOperators
{
    /// <summary>
    /// Interaktionslogik für Button.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("BooleanOperators.Properties.Resources")]
    public partial class ButtonInputPresentation : UserControl
    {

        public event EventHandler StatusChanged;

        public ButtonInputPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Current value of the button
        /// </summary>
        public bool Value { get; set; }

        private void setButton()
        {
            if (Value)
            {
                this.myButton.Background = Brushes.LawnGreen;
                this.myButton.Content = Properties.Resources.True;
            }
            else
            {
                this.myButton.Background = Brushes.Tomato;
                this.myButton.Content = Properties.Resources.False;
            }
        }

        public void update()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setButton();
            }, null);
        }

        public void ExecuteThisMethodWhenButtonIsClicked(object sender, EventArgs e)
        {
            Value = !Value;
            setButton();

            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }
    }
}