using CrypTool.PluginBase;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.NetworkSender
{
    [CrypTool.PluginBase.Attributes.Localization("NetworkSender.Properties.Resources")]
    public partial class NetworkSenderPresentation : UserControl
    {
        private const int MaxStoredPackage = 100;
        private readonly ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();
        private readonly NetworkInput caller;

        public NetworkSenderPresentation(NetworkInput networkInput)
        {
            InitializeComponent();
            DataContext = entries;
            caller = networkInput;
        }
        public void RefreshMetaData(int amountOfSendedPackages)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
            {
                try
                {
                    Amount.Value = amountOfSendedPackages.ToString();
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);
        }

        public void SetStaticMetaData(string starttime, int port)
        {
            string[] jar = new string[2] { starttime, port.ToString() };
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
            {
                try
                {
                    StartTime.Value = jar[0];
                    LisPort.Value = jar[1];
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), jar);
        }

        public void AddPresentationPackage(PresentationPackage package)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Insert(0, package);

                    //Delets old entries from List if the amount is > 100
                    if (entries.Count > MaxStoredPackage)
                    {
                        entries.RemoveAt(entries.Count - 1);
                    }
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), package);

        }

        public void ClearList()
        {

            Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Clear();
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);
        }

        /// <summary>
        ///  invoke presentation in order to  update the speedrate
        ///  </summary>
        public void UpdateSpeedrate(string speedrate)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
            {
                try
                {
                    Speedrate.Value = speedrate;
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);
        }

    }
}
