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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace CrypTool.Plugins.VisualEncoder
{
    public class VisualEncoderSettings : ISettings
    {
        #region Variables

        private readonly List<string> inputList = new List<string>();
        private readonly Dictionary<DimCodeType, List<string>> inputVisibility = new Dictionary<DimCodeType, List<string>>();
        private readonly VisualEncoder caller;

        #region input Variables

        private bool appendICV = true;
        private DimCodeType encodingType;

        #endregion

        public enum DimCodeType { EAN8, EAN13, Code39, Code128, QRCode, PDF417 };

        #endregion

        public VisualEncoderSettings(VisualEncoder caller)
        {
            this.caller = caller;
            //init for each enum inputVisibilityLists 
            foreach (DimCodeType name in Enum.GetValues(typeof(DimCodeType)))
            {
                inputVisibility[name] = new List<string>();
            }

            //add all inputs
            inputList.Add("AppendICV");

            //add input for each codetype if it should be visible
            inputVisibility[DimCodeType.EAN8].Add("AppendICV");
            inputVisibility[DimCodeType.EAN13].Add("AppendICV");
            inputVisibility[DimCodeType.Code39].Add("AppendICV");
            UpdateTaskPaneVisibility();
        }



        #region TaskPane Settings

        [TaskPane("EncodingTypeCaption", "EncodingTypeTooltip", null, 1, true, ControlType.ComboBox, new[] { "EAN8", "EAN13", "Code39", "Code128", "QRCode", "PDF417" })]
        public DimCodeType EncodingType
        {
            get => encodingType;
            set
            {
                if (value != encodingType)
                {
                    encodingType = value;
                    caller.Execute();
                    OnPropertyChanged("EncodingType");
                    UpdateTaskPaneVisibility();
                }
            }
        }


        [TaskPane("AppendICVCaption", "AppendICVTooltip", "BarcodeSection", 2, true, ControlType.CheckBox)]
        public bool AppendICV
        {
            get => appendICV;
            set
            {
                if (appendICV != value)
                {
                    appendICV = value;
                    OnPropertyChanged("AppendICV");
                }
            }
        }


        #endregion

        #region Visualisation updates

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            foreach (TaskPaneAttribteContainer tpac in inputList.Select(input =>
                new TaskPaneAttribteContainer(input, (inputVisibility[EncodingType].Contains(input))
                                                      ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }


        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
