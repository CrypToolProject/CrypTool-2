/*                              
   Copyright Armin Krauss, Martin Franz

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

namespace CrypTool.Plugins.DGK
{
    /// <summary>
    /// Settings class for the DGKKeyGenerator plugin
    /// </summary>
    internal class DGKKeyGeneratorSettings : ISettings
    {

        #region private members

        private int bitSizeK = 512;
        private int bitSizeT = 160;
        private int limitL = 10;

        #endregion

        #region events

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        #endregion

        #region public

        /// <summary>
        /// Getter/Setter for the parameter k
        /// </summary>
        [TaskPane("BitSizeKCaption", "BitSizeKTooltip", null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public int BitSizeK
        {
            get => bitSizeK;
            set
            {
                if (value != bitSizeK)
                {
                    bitSizeK = value;
                    OnPropertyChanged("BitSizeK");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the parameter t
        /// </summary>
        [TaskPane("BitSizeTCaption", "BitSizeTTooltip", null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public int BitSizeT
        {
            get => bitSizeT;
            set
            {
                if (value != bitSizeT)
                {
                    bitSizeT = value;
                    OnPropertyChanged("BitSizeT");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the parameter l
        /// </summary>
        [TaskPane("BitSizeLCaption", "BitSizeLTooltip", null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public int LimitL
        {
            get => limitL;
            set
            {
                if (value != limitL)
                {
                    limitL = value;
                    OnPropertyChanged("BitSizeL");
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

    }//end DGKKeyGeneratorSettings

}//end CrypTool.Plugins.DGK
