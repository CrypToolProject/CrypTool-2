/*
   Copyright 2008-2012 Arno Wacker, University of Kassel

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
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.Convertor
{
    public class StringDecoderSettings : ISettings
    {
        public enum EncodingTypes { UTF8, UTF7, UTF16, UTF32, ASCII, ISO8859_15, Windows1252 };
        public enum PresentationFormat { Text, Binary, Octal, Decimal, Hex, Base64 };

        #region Private variables

        private EncodingTypes encoding = EncodingTypes.UTF8;
        private PresentationFormat presentation = PresentationFormat.Text;
        //private Boolean removeSpaces = false;
        private bool useSeparators = false;
        private string separators = " ,";

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        /// <summary>
        /// Presentation Format property used in the Settings pane. 
        /// </summary>
        [ContextMenu("PresentationFormatSettingCaption", "PresentationFormatSettingTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "PresentationFormatSettingList1", "PresentationFormatSettingList2", "PresentationFormatSettingList3", "PresentationFormatSettingList4", "PresentationFormatSettingList5", "PresentationFormatSettingList6" })]
        [TaskPane("PresentationFormatSettingCaption", "PresentationFormatSettingTooltip", null, 1, false, ControlType.ComboBox, new string[] { "PresentationFormatSettingList1", "PresentationFormatSettingList2", "PresentationFormatSettingList3", "PresentationFormatSettingList4", "PresentationFormatSettingList5", "PresentationFormatSettingList6" })]
        public PresentationFormat PresentationFormatSetting
        {
            get => presentation;
            set
            {
                if (presentation != value)
                {
                    presentation = value;
                    OnPropertyChanged("PresentationFormatSetting");

                    SetVisibilityOfEncoding();
                }
            }
        }

        /// <summary>
        /// Encoding property used in the Settings pane. 
        /// </summary>
        [ContextMenu("EncodingSettingCaption", "EncodingSettingTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        [TaskPane("EncodingSettingCaption", "EncodingSettingTooltip", null, 2, false, ControlType.ComboBox, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        public EncodingTypes Encoding
        {
            get => encoding;
            set
            {
                if (encoding != value)
                {
                    encoding = value;
                    OnPropertyChanged("Encoding");
                }
            }
        }

        /// <summary>
        /// Use a separator to split input
        /// </summary>
        [TaskPane("UseSeparatorsSettingCaption", "UseSeparatorsSettingTooltip", null, 3, false, ControlType.CheckBox)]
        public bool UseSeparators
        {
            get => useSeparators;
            set
            {
                if (useSeparators != value)
                {
                    useSeparators = value;
                    OnPropertyChanged("UseSeparators");
                }
            }
        }

        /// <summary>
        /// Separator characters used to split the input
        /// </summary>
        [TaskPane("SeparatorsSettingCaption", "SeparatorsSettingTooltip", null, 4, false, ControlType.TextBox)]
        public string Separators
        {
            get => separators;
            set
            {
                if (separators != value)
                {
                    separators = value;
                    OnPropertyChanged("Separators");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            SetVisibilityOfEncoding();
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        internal void SetVisibilityOfEncoding()
        {
            if (TaskPaneAttributeChanged != null)
            {
                Visibility visibility;
                visibility = presentation == PresentationFormat.Text ? Visibility.Visible : Visibility.Collapsed;
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Encoding", visibility)));
                visibility = (presentation == PresentationFormat.Text || presentation == PresentationFormat.Base64) ? Visibility.Collapsed : Visibility.Visible;
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseSeparators", visibility)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Separators", visibility)));
            }
        }

        #endregion

    }
}
