/*
   Copyright 2022 Vasily Mikhalev, CrypTool 2 Team

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

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        #region Wizard
        [TaskPane("CurrentStepCaption", "CurrentStepTip", "InfoAndControlGroup", 0, true, ControlType.TextBoxReadOnly)]
        public string HintMessage
        {
            get
            {
                return _hintMessage;
            }
            set
            {
                if (_hintMessage != value)
                {
                    _hintMessage = value;
                    OnPropertyChanged("HintMessage");
                }
            }
        }




        #endregion
    }
}
