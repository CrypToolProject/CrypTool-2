using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.Plugins.KKDFSHA256
{
    /// <summary>
    /// Interaktionslogik für KKDFSHA256Pres.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("KKDFSHA256.Properties.Resources")]
    public partial class KKDFSHA256Pres : UserControl
    {

        public AutoResetEvent buttonNextClickedEvent;
        public AutoResetEvent buttonPrevClickedEvent;
        public AutoResetEvent buttonStartClickedEvent;
        public AutoResetEvent buttonRestartClickedEvent;
        private bool _skipChapter;
        private bool _restart;
        private bool _next;
        private bool _prev;

        /// <summary>
        /// getter, setter for Restart
        /// </summary>
        public bool Restart
        {
            get => _restart;
            set => _restart = value;
        }

        /// <summary>
        /// getter, setter for SkipChapter
        /// </summary>
        public bool SkipChapter
        {
            get => _skipChapter;
            set => _skipChapter = value;
        }

        /// <summary>
        /// getter, setter for Next
        /// </summary>
        public bool Next
        {
            get => _next;
            set => _next = value;
        }

        /// <summary>
        /// getter, setter for Prev
        /// </summary>
        public bool Prev
        {
            get => _prev;
            set => _prev = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public KKDFSHA256Pres()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            buttonPrevClickedEvent = new AutoResetEvent(false);
            buttonStartClickedEvent = new AutoResetEvent(false);
            buttonRestartClickedEvent = new AutoResetEvent(false);
            _skipChapter = false;
            _next = false;
            _prev = false;

        }

        /// <summary>
        /// handles button clickevent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRestart_Click(object sender, RoutedEventArgs e)
        {
            _restart = true;
            buttonRestartClickedEvent.Set();
        }

        /// <summary>
        /// handles button clickevent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            _restart = false;
            buttonStartClickedEvent.Set();
        }

        /// <summary>
        /// handles button clickevent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSkip_Click(object sender, RoutedEventArgs e)
        {
            _skipChapter = true;
            _next = true;
            buttonNextClickedEvent.Set();
        }

        /// <summary>
        /// handles button clickevent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            _next = true;
            buttonNextClickedEvent.Set();
        }

        /// <summary>
        /// handles button clickevent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            _prev = true;
            buttonPrevClickedEvent.Set();
        }
    }
}
