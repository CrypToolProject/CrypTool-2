/*
    Copyright 2013 Christopher Konze, University of Kassel
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.NetworkReceiver
{
    [CrypTool.PluginBase.Attributes.Localization("NetworkReceiver.Properties.Resources")]
    public partial class NetworkReceiverPresentation : UserControl
    {
        public const int MaxStoredPackage = 100;
        private readonly ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();
        private readonly NetworkReceiver caller;


        public NetworkReceiverPresentation(NetworkReceiver networkReceiver)
        {
            InitializeComponent();
            DataContext = entries;
            caller = networkReceiver;
        }

        /// <summary>
        /// invoke presentation in order to set the starttime and the port
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="port"></param>
        public void SetStaticMetaData(string starttime, string port)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
                {
                    try
                    {
                        StartTime.Value = starttime;
                        LisPort.Value = port;
                    }
                    catch (Exception e)
                    {
                        caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                    }
                }), null);
        }

        /// <summary>
        ///  invoke presentation in order to add a new packet and refreshed counters
        ///  </summary>
        /// <param name="package"></param>
        /// <param name="amountOfReceivedPackages"></param>
        /// <param name="amountOfUniqueIps"></param>
        public void UpdatePresentation(PresentationPackage package, int amountOfReceivedPackages, int amountOfUniqueIps)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Insert(0, package);

                    Amount.Value = amountOfReceivedPackages.ToString();
                    UniqueIP.Value = amountOfUniqueIps.ToString();

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
            }), null);

        }

        public void UpdatePresentationClientCount(int amountOfUniqueIps)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
                {
                    try
                    {
                        UniqueIP.Value = amountOfUniqueIps.ToString();
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

        /// <summary>
        /// clears the packet list and resets packet count and uniueIp count
        /// </summary>
        public void ClearPresentation()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Clear();
                    Amount.Value = "0";
                    UniqueIP.Value = "0";
                    Speedrate.Value = "0 B/s";

                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);
        }
    }
}
