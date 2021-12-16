/*
   Copyright CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Text;
using System.Windows;

namespace ImageSteganographyVisualization
{
    public enum ModeType { LSB, BPCS };
    public enum ActionType { Hide, Extract };
    public enum ColorLayerOrder { RGB, RBG, GBR, GRB, BRG, BGR };


    public class ImageSteganographyVisualizationSettings : ISettings
    {
        #region Private Variables

        private ModeType mode = ModeType.LSB;
        private ActionType action = ActionType.Hide;
        private bool showPresentation = false;
        private double complexityThreshold = 0.3;
        private ColorLayerOrder order = ColorLayerOrder.BGR;
        public string redBitmask = "00000001";
        public string greenBitmask = "00000001";
        public string blueBitmask = "00000001";

        #endregion

        public ImageSteganographyVisualizationSettings()
        {
            for (int i = 0; i < 256; i++)
            {
                bitmasksAvailable.Add(GetBinaryString(i));
            }
        }

        #region TaskPane Settings

        [TaskPane("ModeCaption", "ModeTooltip", null, 0, false, ControlType.ComboBox, new[] { "ModeList1", "ModeList2" })]
        public ModeType SelectedMode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;
                    if (value == ModeType.LSB)
                    {
                        ShowSettingsElement("DefaultBitMasks");
                        HideSettingsElement("ComplexityThreshold");
                        HideSettingsElement("SelectedOrder");
                    }
                    else
                    {
                        HideSettingsElement("DefaultBitMasks");
                        ShowSettingsElement("ComplexityThreshold");
                        ShowSettingsElement("SelectedOrder");
                    }
                    OnPropertyChanged("SelectedMode");
                }
            }
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new[] { "ActionList1", "ActionList2" })]
        public ActionType Action
        {
            get => action;
            set
            {
                if (action != value)
                {
                    action = value;
                    if (value == ActionType.Hide)
                    {
                        ShowSettingsElement("ShowPresentation");
                    }
                    else
                    {
                        HideSettingsElement("ShowPresentation");
                        HideSettingsElement("RedBitmask");
                        HideSettingsElement("GreenBitmask");
                        HideSettingsElement("BlueBitmask");
                    }
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("ShowPresentationSettingsCaption", "ShowPresentationSettingsTooltip", null, 2, false, ControlType.CheckBox)]
        public bool ShowPresentation
        {
            get => showPresentation;
            set
            {
                if (showPresentation != value)
                {
                    showPresentation = value;
                    if (value == true || mode == ModeType.BPCS)
                    {
                        HideSettingsElement("RedBitmask");
                        HideSettingsElement("GreenBitmask");
                        HideSettingsElement("BlueBitmask");
                    }
                    else
                    {
                        ShowSettingsElement("RedBitmask");
                        ShowSettingsElement("GreenBitmask");
                        ShowSettingsElement("BlueBitmask");
                    }
                    OnPropertyChanged("ShowPresentation");
                }
            }
        }

        [TaskPane("RedBitMaskLabel", "BitmaskTooltip", null, 3, false, ControlType.DynamicComboBox, new string[] { "BitmasksAvailable" })]
        public string RedBitmask
        {
            get => redBitmask;
            set
            {
                if (value != redBitmask)
                {
                    redBitmask = value;
                    OnPropertyChanged("RedBitmask");
                }
            }
        }

        [TaskPane("GreenBitMaskLabel", "BitmaskTooltip", null, 4, false, ControlType.DynamicComboBox, new string[] { "BitmasksAvailable" })]
        public string GreenBitmask
        {
            get => greenBitmask;
            set
            {
                if (value != greenBitmask)
                {
                    greenBitmask = value;
                    OnPropertyChanged("GreenBitmask");
                }
            }
        }

        [TaskPane("BlueBitMaskLabel", "BitmaskTooltip", null, 5, false, ControlType.DynamicComboBox, new string[] { "BitmasksAvailable" })]
        public string BlueBitmask
        {
            get => blueBitmask;
            set
            {
                if (value != blueBitmask)
                {
                    blueBitmask = value;
                    OnPropertyChanged("BlueBitmask");
                }
            }
        }

        [TaskPane("ComplexityCaption", "ComplexityTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeDouble, 0, 1, 0.01)]
        public double ComplexityThreshold
        {
            get => complexityThreshold;
            set
            {
                if (complexityThreshold != value)
                {
                    complexityThreshold = value;
                    OnPropertyChanged("ComplexityThreshold");
                }
            }
        }

        [TaskPane("ColorOrderCaption", "ColorOrderTooltip", null, 4, false, ControlType.ComboBox, new[] { "RGB", "RBG", "GBR", "GRB", "BRG", "BGR" })]
        public ColorLayerOrder SelectedOrder
        {
            get => order;
            set
            {
                if (order != value)
                {
                    order = value;
                    OnPropertyChanged("SelectedOrder");
                }
            }
        }

        #endregion

        #region Helper methods

        private ObservableCollection<string> bitmasksAvailable = new ObservableCollection<string>();
        [DontSave]
        public ObservableCollection<string> BitmasksAvailable
        {
            get => bitmasksAvailable;
            set
            {
                if (value != bitmasksAvailable)
                {
                    bitmasksAvailable = value;
                    OnPropertyChanged("BitmasksAvailable");
                }
            }
        }

        /// <summary>
        /// Creates the binary string of a provided input integer value
        /// </summary>
        public static string GetBinaryString(int value)
        {
            string s = Convert.ToString(value, 2);
            int fill = 8 - s.Length;
            StringBuilder binaryString = new StringBuilder();
            for (int i = 0; i < fill; i++)
            {
                binaryString.Append("0");
            }
            binaryString.Append(s);
            return binaryString.ToString();
        }

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void HideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        public ModeType GetMode()
        {
            return mode;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {
            // if template or saved workplace is used, display necessary settings elements and hide elements that are not relevant
            if (mode == ModeType.LSB)
            {
                HideSettingsElement("ComplexityThreshold");
                HideSettingsElement("SelectedOrder");
                if (action == ActionType.Extract || showPresentation == true)
                {
                    HideSettingsElement("RedBitmask");
                    HideSettingsElement("GreenBitmask");
                    HideSettingsElement("BlueBitmask");
                }
            }
            else if (mode == ModeType.BPCS)
            {
                HideSettingsElement("RedBitmask");
                HideSettingsElement("GreenBitmask");
                HideSettingsElement("BlueBitmask");

            }
            if (action == ActionType.Extract)
            {
                HideSettingsElement("ShowPresentation");
            }
        }
    }
}
