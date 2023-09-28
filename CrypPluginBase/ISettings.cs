/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.ComponentModel;

namespace CrypTool.PluginBase
{
    public interface ISettings : INotifyPropertyChanged
    {
        /// <summary>
        /// Initialize initalizes the settings of a component. it is called directly after the call of the Initialize call of the component.
        /// This method should be used for example to hide settings values which are disabled by default (for instance at the enigma some
        /// settings are hidden if the enigma is set to analyse mode
        /// </summary>
        void Initialize();
    }
}
