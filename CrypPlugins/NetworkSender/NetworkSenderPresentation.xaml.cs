/*
   Copyright (C) CrypTool 2 Team

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
