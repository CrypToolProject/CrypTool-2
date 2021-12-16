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

namespace CrypTool.Plugins.PaddingOracleAttack
{
    public class PaddingOracleAttackSettings : ISettings
    {
        #region Private Variables

        private int blockSize = 8;
        private readonly int viewByte = 1;

        #endregion

        #region TaskPane Settings

        [TaskPane("BlocksizeCaption", "BlocksizeTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int BlockSize
        {
            get => blockSize;
            set
            {
                blockSize = value;
                OnPropertyChanged("BlockSize");
            }
        }

        /*[TaskPane("ViewByte", "Viewed byte range", null, 2, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int ViewByte
        {
            get { return viewByte; }
            set
            {
                viewByte = value;
                
                OnPropertyChanged("ViewByte");
            }
        }*/



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

        public event System.ComponentModel.PropertyChangedEventHandler ButtonClicked;

        private void OnButtonClicked(string propertyName)
        {
            ButtonClicked(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
