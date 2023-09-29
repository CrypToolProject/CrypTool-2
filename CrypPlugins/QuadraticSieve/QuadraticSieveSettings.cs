/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.QuadraticSieve
{
    internal class QuadraticSieveSettings : ISettings
    {
        #region private variables
        private int coresUsed;
        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        private bool deleteCache;
        private bool usePeer2Peer;
        private string channel;
        private readonly QuadraticSieve quadraticSieve;
        #endregion

        #region events
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        #endregion

        #region public

        /// <summary>
        /// Constructs a new QuadraticSieveSettings
        /// 
        /// Also calculates the amount of cores which can be used for the quadratic sieve
        /// </summary>
        public QuadraticSieveSettings(QuadraticSieve quadraticSieve)
        {
            this.quadraticSieve = quadraticSieve;
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                CoresAvailable.Add((i + 1).ToString());
            }

            CoresUsed = Environment.ProcessorCount - 1;
        }

        public void Initialize()
        {
            checkAndSetVisibility();
        }

        /// <summary>
        /// Getter/Setter for the amount of cores which the user wants to have used by the quadratic sieve
        /// </summary>
        [TaskPane("CoresUsedCaption", "CoresUsedTooltip", null, 1, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get => coresUsed;
            set
            {
                if (value != coresUsed)
                {
                    coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        /// <summary>
        /// Get the available amount of cores of this pc
        /// </summary>
        [DontSave]
        public ObservableCollection<string> CoresAvailable
        {
            get => coresAvailable;
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                    OnPropertyChanged("CoresAvailable");
                }
            }
        }

        /// <summary>
        /// Getter / Setter to enable/disable the deletion of the cache
        /// </summary>
        [TaskPane("DeleteCacheCaption", "DeleteCacheTooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool DeleteCache
        {
            get => deleteCache;
            set
            {
                if (value != deleteCache)
                {
                    deleteCache = value;
                    OnPropertyChanged("DeleteCache");
                }
            }
        }

        /// <summary>
        /// Getter / Setter to enable/disable the use of peer2peer
        /// </summary>
        //[TaskPane( "UsePeer2PeerCaption", "UsePeer2PeerTooltip", null, 3, false, ControlType.CheckBox, "", null)]
        public bool UsePeer2Peer
        {
            get
            {
                if (false)
                {
                    UsePeer2Peer = false;
                    return false;
                }
                return usePeer2Peer;
            }
            set
            {
                if (value != usePeer2Peer)
                {
                    usePeer2Peer = value;
                    checkAndSetVisibility();
                    OnPropertyChanged("UsePeer2Peer");
                }
            }
        }

        private void checkAndSetVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            //TODO CKONZE reenable p2p
            if (false)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UsePeer2Peer", Visibility.Collapsed)));
            }

            if (UsePeer2Peer)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Channel", Visibility.Visible)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Channel", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("StatusKeyButton", Visibility.Collapsed)));
            }
        }

        /// <summary>
        /// Channel of the Peer2Peer network
        /// </summary>
        [TaskPane("ChannelCaption", "ChannelTooltip", null, 4, false, ControlType.TextBox, "", null)]
        public string Channel
        {
            get => channel;
            set
            {
                if (value != channel)
                {
                    channel = value;
                    OnPropertyChanged("Channel");
                }
            }
        }

        [TaskPane("StatusKeyButtonCaption", "StatusKeyButtonTooltip", null, 5, true, ControlType.Button)]
        public void StatusKeyButton()
        {

            if (!quadraticSieve.Running)
            {
                quadraticSieve.GuiLogMessage("Quadratic sieve must be running to copy the status key.", NotificationLevel.Error);
                return;
            }
            if (!usePeer2Peer)
            {
                quadraticSieve.GuiLogMessage("You don't need the status key if you don't want to use peer2peer.", NotificationLevel.Error);
                return;
            }

            //TODO CKONZE reenable p2p
            /*
                 var statusKey = quadraticSieve.PeerToPeer.StatusKey();

                 Clipboard.SetDataObject(statusKey, true);
                    
                 quadraticSieve.GuiLogMessage("Status key '" + statusKey + "' has been copied to clipboard.",
                                      NotificationLevel.Info);
             */
        }

        #endregion

        #region private

        /// <summary>
        /// A property changed
        /// </summary>
        /// <param name="name">name</param>
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}
