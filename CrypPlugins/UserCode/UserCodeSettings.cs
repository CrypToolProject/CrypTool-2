/*                              
   Copyright 2011, Nils Kopal, Uni Duisburg-Essen

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

namespace CrypTool.Plugins.UserCode
{
    internal class UserCodeSettings : ISettings
    {
        private string _Sourcecode = "";
        //[TaskPane("NumberCaption", "NumberTooltip", null, 1, false, ControlType.TextBox)]
        public string Sourcecode
        {
            get => _Sourcecode;
            set
            {
                _Sourcecode = value;
                OnPropertyChanged("Number");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

    }
}
