using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for UpdaterPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class UpdaterPresentation : UserControl
    {
        public delegate void RestartClickedHandler();
        public event RestartClickedHandler OnRestartClicked;
        private static UpdaterPresentation singleton = null;
        
        private UpdaterPresentation()
        {
            InitializeComponent();
            Tag = FindResource("NoUpdate");
        }

        public static UpdaterPresentation GetSingleton()
        {
            if (singleton == null)
            {
                singleton = new UpdaterPresentation();
            }
            return singleton;
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            switch (AutoUpdater.GetSingleton().CurrentState)
            {
                case AutoUpdater.State.Idle:
                    AutoUpdater.GetSingleton().BeginCheckingForUpdates('M');
                    break;
                case AutoUpdater.State.Checking:
                    break;
                case AutoUpdater.State.UpdateAvailable:
                    AutoUpdater.GetSingleton().Download();
                    break;
                case AutoUpdater.State.Downloading:
                    break;
                case AutoUpdater.State.UpdateReady:
                    if (AutoUpdater.GetSingleton().IsUpdateReady)
                    {
                        OnRestartClicked();
                    }
                    break;
            }
        }        

        public void FillChangelogText(string changelogText)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ChangelogText.Html = changelogText;
                    }
                    catch (Exception)
                    {
                        //Uncritical failure: Do nothing
                    }
                }, null);
            }
            catch (Exception)
            {
                //Uncritical failure: Do nothing
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.Text, ChangelogText.Html);
            }
            catch (Exception)
            {
                // same here as above
            }
        }
    }   
}
