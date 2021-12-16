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
using BlockmodeVisualizer;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.BlockmodeVisualizer
{
    public class BlockmodeVisualizerSettings : ISettings
    {
        #region Private Variables

        private Blockmodes blockmode;

        #endregion

        #region TaskPane Settings

        [TaskPane("action_select", "action_select_tooltip", null, 1, false, ControlType.ComboBox,
            "encrypt", "decrypt")]
        public Actions Action { get; set; }

        [TaskPane("blockmode_select", "blockmode_select_tooltip", null, 0, false, ControlType.ComboBox,
            "ecb", "cbc", "cfb", "ofb", "ctr", "xts", "ccm", "gcm")]
        public Blockmodes Blockmode
        {
            get => blockmode;
            set
            {
                if (blockmode == value)
                {
                    return;
                }

                blockmode = value;
                UpdateTaskPaneVisibility();
            }
        }

        [TaskPane("datasegment_length_input", "datasegment_length_input_tooltip", null, 3, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 64)]
        public int DataSegmentLength { get; set; }

        [TaskPane("padding_select", "padding_select_tooltip", null, 2, false, ControlType.ComboBox,
            "none", "zeros", "pkcs7", "ansix923", "iso10126", "onezeros")]
        public BlockCipherHelper.PaddingType Padding { get; set; }

        [TaskPane("tag_length", "tag_length_tooltip", null, 4, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 64)]
        public int TagLength { get; set; }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        /*
         * Show only those fields which are necessary for the selected mode of operation.
         */
        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            switch (Blockmode)
            {
                case Blockmodes.ECB:
                case Blockmodes.CBC:
                    ShowSettingsElement("Padding");
                    HideSettingsElement("DataSegmentLength");
                    HideSettingsElement("TagLength");
                    break;
                case Blockmodes.CFB:
                    ShowSettingsElement("Padding");
                    ShowSettingsElement("DataSegmentLength");
                    HideSettingsElement("TagLength");
                    break;
                case Blockmodes.OFB:
                case Blockmodes.CTR:
                case Blockmodes.XTS:
                    HideSettingsElement("Padding");
                    HideSettingsElement("DataSegmentLength");
                    HideSettingsElement("TagLength");
                    break;
                case Blockmodes.CCM:
                case Blockmodes.GCM:
                    HideSettingsElement("Padding");
                    HideSettingsElement("DataSegmentLength");
                    ShowSettingsElement("TagLength");
                    break;
                default:
                    throw new InvalidOperationException(Properties.Resources.ResourceManager.GetString("should_not_happen_exception".ToString()));
            }
        }

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }

        }

        private void HideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion

        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }
    }
}
