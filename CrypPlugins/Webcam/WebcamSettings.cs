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
using DirectShowLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management;

namespace CrypTool.Plugins.Webcam
{
    public class WebcamSettings : ISettings
    {
        #region Private Variables

        private int _quality = 50;
        private int _brightness = 100;
        private int _contrast = 25;
        private int _sharpness = 100;

        private ObservableCollection<string> device = new ObservableCollection<string>();
        private int capDevice;
        private int _captureFrequency = 25;
        private int _takeImageChoice;

        #endregion

        public WebcamSettings()
        {
            CameraDevices.Clear();
            DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int cameraIndex = 0; cameraIndex < captureDevices.Length; cameraIndex++)
            {
                CameraDevices.Add($"{cameraIndex}: {captureDevices[cameraIndex].Name}");
            }

        }

        [DontSave]
        public ObservableCollection<string> CameraDevices
        {
            get => device;
            set
            {
                if (value != device)
                {
                    device = value;
                    OnPropertyChanged("CameraDevices");
                }
            }
        }

        #region TaskPane Settings

        [TaskPane("DeviceChoiceCaption", "DeviceChoiceTooltip", null, 0, false, ControlType.DynamicComboBox, new string[] { "CameraDevices" })]
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

        [TaskPane("TakeImageCaption", "TakeImageTooltip", null, 0, false, ControlType.ComboBox, new string[] { "TakeImageChoiceList1", "TakeImageChoiceList2", "TakeImageChoiceList3" })]
        public int TakeImageChoice
        {
            get => _takeImageChoice;
            set
            {
                if (_takeImageChoice != value)
                {
                    _takeImageChoice = value;
                    OnPropertyChanged("TakeImageChoice");
                }
            }
        }

        [TaskPane("ImageQualityCaption", "ImageQualityTooltip", "DeviceSettingsGroup", 1, true, ControlType.Slider, 1, 100)]
        public int ImageQuality
        {
            get => _quality;
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    OnPropertyChanged("ImageQuality");
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

        [TaskPane("CaptureFrequencyCaption", "CaptureFrequencyTooltip", "DeviceSettingsGroup", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 250)]
        public int CaptureFrequency
        {
            get => _captureFrequency;
            set
            {
                if (_captureFrequency != value)
                {
                    _captureFrequency = value;
                    OnPropertyChanged("SendImage");
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
