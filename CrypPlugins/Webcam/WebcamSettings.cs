/*
   Copyright 2019 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management;

namespace CrypTool.Plugins.Webcam
{
    public class WebcamSettings : ISettings
    {
        #region Private Variables

        private int quality = 50;
        private int _brightness = 100;
        private int _contrast = 25;
        private int _sharpness = 100;

        private ObservableCollection<string> device = new ObservableCollection<string>();
        private int capDevice;
        private int sendPicture = 1000;
        private int takePictureChoice;

        #endregion

        public WebcamSettings()
        {
            Device.Clear();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')");
            foreach (ManagementBaseObject info in searcher.Get())
            {
                string name = Convert.ToString(info["Caption"]);
                Device.Add(name);
            }


            //devices hinzufügen
            //standard camera auswählen
        }

        public ObservableCollection<string> Device
        {
            get => device;
            set
            {
                if (value != device)
                {
                    device = value;
                    OnPropertyChanged("Device");
                }
            }
        }

        #region TaskPane Settings

        [TaskPane("DeviceChoiceCaption", "DeviceChoiceTooltip", null, 0, false, ControlType.DynamicComboBox, new string[] { "Device" })]
        public int DeviceChoice
        {
            get => capDevice;
            set
            {
                if (capDevice != value)
                {
                    capDevice = value;
                    OnPropertyChanged("DeviceChoice");
                }
            }
        }

        [TaskPane("TakePictureChoiceCaption", "TakePictureChoiceTooltip", null, 0, false, ControlType.ComboBox, new string[] { "TakePictureChoiceList1", "TakePictureChoiceList2", "TakePictureChoiceList3" })]
        public int TakePictureChoice
        {
            get => takePictureChoice;
            set
            {
                if (takePictureChoice != value)
                {
                    takePictureChoice = value;
                    OnPropertyChanged("TakePictureChoice");
                }
            }
        }

        [TaskPane("PictureQualityCaption", "PictureQualityTooltip", "DeviceSettingsGroup", 1, true, ControlType.Slider, 1, 100)]
        public int PictureQuality
        {
            get => quality;
            set
            {
                if (quality != value)
                {
                    quality = value;
                    OnPropertyChanged("PictureQuality");
                }
            }
        }

        [TaskPane("BrightnessCaption", "BrightnessTooltip", "DeviceSettingsGroup", 2, true, ControlType.Slider, 1, 100)]
        public int Brightness
        {
            get => _brightness;
            set
            {
                if (_brightness != value)
                {
                    _brightness = value;
                    OnPropertyChanged("Brightness");
                }
            }
        }

        [TaskPane("ContrastCaption", "ContrastTooltip", "DeviceSettingsGroup", 3, true, ControlType.Slider, 1, 100)]
        public int Contrast
        {
            get => _contrast;
            set
            {
                if (_contrast != value)
                {
                    _contrast = value;
                    OnPropertyChanged("Contrast");
                }
            }
        }

        [TaskPane("SharpnessCaption", "SharpnessTooltip", "DeviceSettingsGroup", 4, true, ControlType.Slider, 1, 100)]
        public int Sharpness
        {
            get => _sharpness;
            set
            {
                if (_sharpness != value)
                {
                    _sharpness = value;
                    OnPropertyChanged("Sharpness");
                }
            }
        }

        [TaskPane("SendPictureCaption", "SendPictureTooltip", "DeviceSettingsGroup", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 40, 10000)]
        public int SendPicture
        {
            get => sendPicture;
            set
            {
                if (sendPicture != value)
                {
                    sendPicture = value;
                    OnPropertyChanged("SendPicture");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
