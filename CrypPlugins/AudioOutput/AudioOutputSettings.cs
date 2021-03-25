/*
   Copyright 2015 Nils Kopal, Applied Information Security, University of Kassel

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
using System.Collections.ObjectModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using NAudio.Wave;

namespace CrypTool.Plugins.AudioOutput
{
    public class AudioOutputSettings : ISettings
    {
        public AudioOutputSettings()
        {
            Devices.Clear();
            var waveInDevices = WaveOut.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                var deviceInfo = WaveOut.GetCapabilities(waveInDevice);
                Devices.Add(String.Format("{0}: {1}, {2} channels", waveInDevice, deviceInfo.ProductName, deviceInfo.Channels));
            }
        }

        public void Initialize()
        {
           
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<string> devices = new ObservableCollection<string>();
        private int device = 0;
        private int buffersize = 500;

        public ObservableCollection<string> Devices
        {
            get { return devices; }
            set
            {
                if (value != devices)
                {
                    devices = value;
                    OnPropertyChanged("Devices");
                }
            }
        }

        [TaskPane("DeviceChoiceCaption", "DeviceChoiceToolTip", null, 0, false, ControlType.DynamicComboBox, new string[] { "Devices" })]
        public int DeviceChoice
        {
            get
            {
                return device;
            }
            set
            {
                if (device != value)
                {
                    device = value;
                    OnPropertyChanged("DeviceChoice");
                }
            }
        }

        [TaskPane("BufferSizeCaption", "BufferSizeTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 500, 5000)]
        public int BufferSize
        {
            get { return buffersize; }
            set
            {
                if ((value) != buffersize)
                {
                    this.buffersize = value;
                    OnPropertyChanged("BufferSize");
                }
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
    }
}
