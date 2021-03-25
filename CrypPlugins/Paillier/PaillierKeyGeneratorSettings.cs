/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows;


namespace CrypTool.Plugins.Paillier
{
    /// <summary>
    /// Settings class for the PaillierKeyGenerator plugin
    /// </summary>
    class PaillierKeyGeneratorSettings : ISettings
    {

        #region private members

        private int source;
        private String p = "127";
        private String q = "521";
        private String keyBitLength = "1024";
        
        #endregion

        #region events
        
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        #endregion

        #region public

        /// <summary>
        /// Getter/Setter for the source of the Key Data
        /// </summary>
        [TaskPane( "SourceCaption", "SourceTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SourceList1", "SourceList2" })]
        public int Source
        {
            get { return this.source; }
            set
            {
                if (((int)value) != source)
                {
                    this.source = (int)value;

                    UpdateTaskPaneVisibility();

                    OnPropertyChanged("Source");   
                }
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (source)
            {
                case 0: // manually enter primes
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeyBitLength", Visibility.Collapsed)));
                    break;
                case 1: // randomly generated primes
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeyBitLength", Visibility.Visible)));
                    break;
            }
        }

        /// <summary>
        /// Getter/Setter for prime P
        /// </summary>
        [TaskPane("PCaption", "PTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public String P
        {
            get { return p; }
            set
            {
                p = value;
                OnPropertyChanged("P");
            }
        }
        
        /// <summary>
        /// Getter/Setter for the prime Q
        /// </summary>
        [TaskPane("QCaption", "QTooltip", null, 3, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public String Q
        {
            get { return q; }
            set
            {
                q = value;
                OnPropertyChanged("Q");
            }
        }

        /// <summary>
        /// Getter/Setter for the bitlength of the randomly generated primes
        /// </summary>
        [TaskPane("KeyBitLengthCaption", "KeyBitLengthTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public String KeyBitLength
        {
            get { return this.keyBitLength; }
            set
            {
                if (value != this.keyBitLength)
                {
                    this.keyBitLength = value;
                    OnPropertyChanged("KeyBitLength");
                }
            }
        }

        #endregion

        #region private

        /// <summary>
        /// The property p changed
        /// </summary>
        /// <param name="p">p</param>
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

    }//end PaillierKeyGeneratorSettings

}//end CrypTool.Plugins.Paillier
