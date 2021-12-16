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
using System;
using System.ComponentModel;

namespace CrypTool.Plugins.ZeroKnowledgeChecker
{

    public class ZeroKnowledgeCheckerSettings : ISettings
    {
        #region Private Variables

        private int amountOfAttempts = 10;
        private int amountOfOptions = 2;

        #endregion

        #region Private init

        public void Initialize()
        {
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("AmountOfAttemptsCaption", "AmountOfAttemptsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int AmountOfAttempts
        {
            get => amountOfAttempts;
            set
            {
                if (amountOfAttempts != value)
                {
                    amountOfAttempts = Math.Max(value, 0);
                    OnPropertyChanged("AmountOfAttempts");
                }
            }
        }


        [TaskPane("_AmountOfOptionsCaption", "_AmountOfOptionsTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int _AmountOfOptions
        {
            get => amountOfOptions;
            set
            {
                if (amountOfOptions != value)
                {
                    amountOfOptions = Math.Max(value, 1);
                    OnPropertyChanged("_AmountOfOptions");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}