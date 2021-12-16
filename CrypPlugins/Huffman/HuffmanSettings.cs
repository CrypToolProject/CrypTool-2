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

namespace CrypTool.Plugins.Huffman
{
    public class HuffmanSettings : ISettings
    {

        #region Public interface

        public enum HuffmanMode { Compress, Decompress };
        public enum PresentationFormat { Text, Binary };
        public enum EncodingTypes { UTF8, UTF7, UTF16, UTF32, ASCII, ISO8859_15, Windows1252 };

        #endregion

        #region Private Variables

        private HuffmanMode action = HuffmanMode.Compress;
        private PresentationFormat presentation = PresentationFormat.Text;
        private EncodingTypes encoding = EncodingTypes.UTF8;

        #endregion

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox,
            new string[] { "ActionList1", "ActionList2" })]
        public HuffmanMode Action
        {
            get => action;
            set
            {
                if (action != value)
                {
                    action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("PresentationCaption", "PresentationTooltip", null, 2, false, ControlType.ComboBox,
            new string[] { "PresentationList1", "PresentationList2" })]
        public PresentationFormat Presentation
        {
            get => presentation;
            set
            {
                if (presentation != value)
                {
                    presentation = value;
                    OnPropertyChanged("Presentation");

                    SetVisibilityOfEncoding();
                }
            }
        }

        [TaskPane("EncodingCaption", "EncodingTooltip", null, 3, false, ControlType.ComboBox,
            new string[] { "EncodingList1", "EncodingList2", "EncodingList3", "EncodingList4",
                "EncodingList5", "EncodingList6", "EncodingList7" })]
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

        #endregion

        #region Events        

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        internal void SetVisibilityOfEncoding()
        {
            if (TaskPaneAttributeChanged != null)
            {
                switch (Presentation)
                {
                    case PresentationFormat.Text:
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs
                    (new TaskPaneAttribteContainer("Encoding", Visibility.Visible)));
                        break;
                    case PresentationFormat.Binary:
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs
                    (new TaskPaneAttribteContainer("Encoding", Visibility.Collapsed)));
                        break;
                }
            }
        }

        #endregion

        public void Initialize()
        {

        }
    }
}
