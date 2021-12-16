using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CrypTool.MD5.Presentation.States
{
    /// <summary>
    /// Interaktionslogik für StartingRoundStepPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.MD5.Properties.Resources")]
    public partial class RoundStepPresentation : UserControl
    {
        public RoundStepPresentation()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ((Storyboard)FindResource("LineFadeStoryboard")).Begin();
            }
        }
    }
}
