/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.Decimalization
{
    public class DecimalizationSettings : ISettings
    {
        #region Private Variables

        private int mode = 0;
        private int quant = 0;
        private readonly string assocString = "Association Table";
        private int ibmA = 0, ibmB = 1, ibmC = 2, ibmD = 3, ibmE = 4, ibmF = 5;


        #endregion

        #region Initialization / Constructor

        public void Initialize()
        {
            switch (mode)
            {
                case 0:
                case 1:
                case 2:
                    hideSettingsElement("AssocString");
                    hideSettingsElement("IbmA"); hideSettingsElement("IbmB"); hideSettingsElement("IbmC"); hideSettingsElement("IbmD"); hideSettingsElement("IbmE"); hideSettingsElement("IbmF");
                    break;
                case 3:
                    showSettingsElement("AssocString");
                    showSettingsElement("IbmA"); showSettingsElement("IbmB"); showSettingsElement("IbmC"); showSettingsElement("IbmD"); showSettingsElement("IbmE"); showSettingsElement("IbmF");
                    break;
                default:
                    break;
            }

        }

        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(1)]
        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
        public int Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;

                    switch (mode)
                    {
                        case 0:
                        case 1:
                        case 2:
                            hideSettingsElement("AssocString");
                            hideSettingsElement("IbmA"); hideSettingsElement("IbmB"); hideSettingsElement("IbmC"); hideSettingsElement("IbmD"); hideSettingsElement("IbmE"); hideSettingsElement("IbmF");
                            break;
                        case 3:
                            showSettingsElement("AssocString");
                            showSettingsElement("IbmA"); showSettingsElement("IbmB"); showSettingsElement("IbmC"); showSettingsElement("IbmD"); showSettingsElement("IbmE"); showSettingsElement("IbmF");
                            break;
                        default:
                            break;
                    }

                    OnPropertyChanged("Mode");
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane("QuantCaption", "QuantTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Quant
        {
            get => quant;
            set
            {
                if (quant != value)
                {
                    quant = value;
                    OnPropertyChanged("Quant");
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPaneAttribute("AssocStringCaption", "AssocStringTooltip", null, 3, true, ControlType.TextBoxReadOnly)]
        public string AssocString
        {
            get => assocString;
            set
            {
                /*if (!assocString.Equals(value))
                {
                    assocString = value;
                    OnPropertyChanged("AssocString");
                }*/
            }
        }

        [PropertySaveOrder(4)]
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("IBMACaption", "IBMATooltip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmA
        {
            get => ibmA;
            set
            {
                if (ibmA != value)
                {
                    ibmA = value;
                    OnPropertyChanged("IbmA");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("IBMBCaption", "IBMBTooltip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmB
        {
            get => ibmB;
            set
            {
                if (ibmB != value)
                {
                    ibmB = value;
                    OnPropertyChanged("IbmB");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("IBMCCaption", "IBMCTooltip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmC
        {
            get => ibmC;
            set
            {
                if (ibmC != value)
                {
                    ibmC = value;
                    OnPropertyChanged("IbmC");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("IBMDCaption", "IBMDTooltip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmD
        {
            get => ibmD;
            set
            {
                if (ibmD != value)
                {
                    ibmD = value;
                    OnPropertyChanged("IbmD");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("IBMECaption", "IBMETooltip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmE
        {
            get => ibmE;
            set
            {
                if (ibmE != value)
                {
                    ibmE = value;
                    OnPropertyChanged("IbmE");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("IBMFCaption", "IBMFTooltip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmF
        {
            get => ibmF;
            set
            {
                if (ibmF != value)
                {
                    ibmF = value;
                    OnPropertyChanged("IbmF");
                }
            }
        }

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Public Events and Methods

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion
    }
}
