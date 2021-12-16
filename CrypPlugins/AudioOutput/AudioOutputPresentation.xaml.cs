using OxyPlot;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CrypTool.Plugins.AudioOutput
{
    /// <summary>
    /// Interaktionslogik für AudioOutputPresentation.xaml
    /// </summary>
    public partial class AudioOutputPresentation : UserControl
    {
        public List<DataPoint> Points { get; private set; } = new List<DataPoint>();

        public AudioOutputPresentation()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
