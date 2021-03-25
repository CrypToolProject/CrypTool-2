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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.NetworkReceiver
{
    public class NetworkReceiverSettings : ISettings
    {
        public static int udpProtocol = 0;
        public static int tcpProtocol = 1;

        #region Private Variables

        private int port; 
        private string deviceIp;
        private bool networkDevice;
        private readonly NetworkReceiver caller;
        private bool byteAsciiSwitch;
        private bool connectionSwitch;
        private int protocol;
        private int numberOfClients;
        private int speedrateIntervall;

        public NetworkReceiverSettings(NetworkReceiver caller)
        {
            this.caller = caller;
           
        }

        public void Initialize()
        {
            NetworkDevice = true;
            UpdateTaskPaneVisibility();
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("ConnectionCaption", "ConnectionTooltip", "NetworkConditionsGroup", 0, false, ControlType.CheckBox)]
        public bool ConnectionSwitch
        {
            get { return connectionSwitch; }
            set
            {
                if (value != connectionSwitch)
                {
                    connectionSwitch = value;
                    OnPropertyChanged("ConnectionSwitch");
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("DeviceIpCaption", "DeviceIpTooltip", "NetworkConditionsGroup", 1, false, ControlType.TextBox)]
        public string DeviceIp
        {
            get { return deviceIp; }
            set
            {
                deviceIp = value;
                OnPropertyChanged("DeviceIp");
            }
        }
     
        [TaskPane("NetworkDeviceCaption", "NetworkDeviceTooltip", "NetworkConditionsGroup", 2, false, ControlType.CheckBox)]
        public bool NetworkDevice
        {
            get { return networkDevice; }
            set
            {
                if (value != networkDevice)
                {
                    if(!value)
                    {
                        var interfaces = getInterfaceIps();
                        if (interfaces.Contains(DeviceIp))
                        {
                            networkDevice = false; 
                        }
                        else
                        {
                            caller.GuiLogMessage("Interface IP not Available", NotificationLevel.Warning);
                            foreach (var @interface in interfaces)
                            {
                                caller.GuiLogMessage("interface: " + @interface, NotificationLevel.Info);
                            }
                            networkDevice = true;
                        }
                    }
                    else
                    {
                        networkDevice = true;
                    }
                    OnPropertyChanged("NetworkDevice");
                }
            }
        }

        [TaskPane("PortCaption", "PortTooltip", "NetworkConditionsGroup", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 65535)]
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                if (port != value)
                {
                    port = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        [TaskPane("ProtocolCaption", "ProtocolTooltip", "NetworkConditionsGroup", 4, false, ControlType.ComboBox, new[] { "UDP", "TCP" })]
        public int Protocol
        {
            get
            {
                return protocol;
            }
            set
            {
                if (protocol != value)
                {
                    protocol = value;
                    OnPropertyChanged("Protocol");
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("ByteAsciiSwitchCaption", "ByteAsciiSwitchTooltip", "PresentationSettingsGroup" , 3, false, ControlType.CheckBox)]
        public bool ByteAsciiSwitch
        {
            get { return byteAsciiSwitch; }
            set
            {
                if (value != byteAsciiSwitch)
                {
                    byteAsciiSwitch = value;
                    OnPropertyChanged("ByteAsciiSwitch");
                }
            }
        } 
     
        [TaskPane("NumberOfClientsCaption", "NumberOfClientsTooltip", "TCPServerConditionsGroup", 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int NumberOfClients
        {
            get
            {
                return numberOfClients;
            }
            set
            {
                if (numberOfClients != value)
                {
                    numberOfClients = value;
                    OnPropertyChanged("NumberOfClients");
                }
            }
        }

        private void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            var tba = new TaskPaneAttribteContainer("NumberOfClients", protocol == udpProtocol ? Visibility.Collapsed : Visibility.Visible);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tba));

            tba = new TaskPaneAttribteContainer("DeviceIp", ConnectionSwitch ? Visibility.Collapsed : Visibility.Visible);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tba));

            tba = new TaskPaneAttribteContainer("NetworkDevice", ConnectionSwitch ? Visibility.Collapsed : Visibility.Visible);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tba));

            tba = new TaskPaneAttribteContainer("Port", ConnectionSwitch ? Visibility.Collapsed : Visibility.Visible);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tba));

            tba = new TaskPaneAttribteContainer("Protocol", ConnectionSwitch ? Visibility.Collapsed : Visibility.Visible);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tba));
        }
        
        /// <summary>
        /// returns a list with all available networkinterfaces
        /// </summary>
        /// <returns></returns>
        private List<String> getInterfaceIps()
        {
            var interfaces = new List<string>();
                            
           foreach (var netInf in NetworkInterface.GetAllNetworkInterfaces())
           {
               var a = netInf.GetIPProperties().UnicastAddresses;
               foreach (var i in a)
               {
                   if (i.Address.AddressFamily == AddressFamily.InterNetwork)
                   {
                       interfaces.Add(i.Address.ToString());
                   }
               }
           }
           return interfaces;
        } 
            
        #endregion
    
        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
    
        public event PropertyChangedEventHandler PropertyChanged;
     

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
      
        #endregion

    }
}
